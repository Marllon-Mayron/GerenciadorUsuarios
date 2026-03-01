using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.DTOs;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums;
using UserManagement.Domain.Interfaces;

namespace UserManagement.Application.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;

        public UserService(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            ITokenService tokenService)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
        }

        public async Task<UserDto> GetByIdAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user == null ? null : MapToDto(user);
        }

        public async Task<UserDto> GetByEmailAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            return user == null ? null : MapToDto(user);
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Select(MapToDto);
        }

        public async Task<UserDto> CreateAsync(CreateUserDto createUserDto)
        {
            // Verificar se email já existe
            if (await _userRepository.EmailExistsAsync(createUserDto.Email))
                throw new InvalidOperationException("Email já está em uso");

            var user = new User
            {
                Name = createUserDto.Name,
                Email = createUserDto.Email,
                PasswordHash = _passwordHasher.HashPassword(createUserDto.Password),
                Role = UserRole.User
            };

            var createdUser = await _userRepository.CreateAsync(user);
            return MapToDto(createdUser);
        }

        public async Task<UserDto> UpdateAsync(Guid id, UpdateUserDto updateUserDto)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                throw new KeyNotFoundException("Usuário não encontrado");

            if (user.Email != updateUserDto.Email &&
                await _userRepository.EmailExistsAsync(updateUserDto.Email))
                throw new InvalidOperationException("Email já está em uso");

            user.Name = updateUserDto.Name;
            user.Email = updateUserDto.Email;
            user.UpdateTimestamp();

            var updatedUser = await _userRepository.UpdateAsync(user);
            return MapToDto(updatedUser);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _userRepository.DeleteAsync(id);
        }

        public async Task<UserDto> ChangeStatusAsync(Guid id, bool activate)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                throw new KeyNotFoundException("Usuário não encontrado");

            if (activate)
                user.Activate();
            else
                user.Deactivate();

            var updatedUser = await _userRepository.UpdateAsync(user);
            return MapToDto(updatedUser);
        }

        public async Task<LoginResponseDto> AuthenticateAsync(LoginDto loginDto)
        {
            var user = await _userRepository.GetByEmailAsync(loginDto.Email);

            if (user == null)
                throw new UnauthorizedAccessException("Email ou senha inválidos");

            if (!user.CanAuthenticate())
                throw new UnauthorizedAccessException("Usuário inativo. Não é possível autenticar");

            if (!_passwordHasher.VerifyPassword(loginDto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Email ou senha inválidos");

            var token = _tokenService.GenerateToken(user);

            return new LoginResponseDto
            {
                Token = token,
                User = MapToDto(user)
            };
        }

        //Método para promover usuário a admin
        public async Task<UserDto> PromoteToAdminAsync(Guid id, Guid adminUserId)
        {
            var adminUser = await _userRepository.GetByIdAsync(adminUserId);
            if (adminUser == null || !adminUser.IsAdmin())
                throw new UnauthorizedAccessException("Apenas administradores podem promover usuários");

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                throw new KeyNotFoundException("Usuário não encontrado");

            if (user.IsAdmin())
                throw new InvalidOperationException("Usuário já é administrador");

            user.Role = UserRole.Admin;
            user.UpdateTimestamp();

            var updatedUser = await _userRepository.UpdateAsync(user);
            return MapToDto(updatedUser);
        }

        //Método para rebaixar admin a user
        public async Task<UserDto> DemoteToUserAsync(Guid id, Guid adminUserId)
        {
            var adminUser = await _userRepository.GetByIdAsync(adminUserId);
            if (adminUser == null || !adminUser.IsAdmin())
                throw new UnauthorizedAccessException("Apenas administradores podem rebaixar usuários");

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                throw new KeyNotFoundException("Usuário não encontrado");

            if (user.IsUser())
                throw new InvalidOperationException("Usuário já é usuário comum");

            user.Role = UserRole.User;
            user.UpdateTimestamp();

            var updatedUser = await _userRepository.UpdateAsync(user);
            return MapToDto(updatedUser);
        }

        private UserDto MapToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Status = user.Status.ToString(),
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }

        public async Task<(IEnumerable<UserDto> Users, int TotalCount)> GetAllPaginatedAsync(int pageNumber, int pageSize)
        {
            try
            {
                Console.WriteLine($"UserService.GetAllPaginatedAsync - Página: {pageNumber}, Tamanho: {pageSize}");

                var (users, totalCount) = await _userRepository.GetAllPaginatedAsync(pageNumber, pageSize);

                Console.WriteLine($"Repository retornou {users.Count()} usuários de {totalCount} total");

                var userDtos = users.Select(MapToDto).ToList();

                return (userDtos, totalCount);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERRO no GetAllPaginatedAsync: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<UserStatisticsDto> GetUserStatisticsAsync()
        {
            try
            {
                Console.WriteLine("UserService.GetUserStatisticsAsync - Buscando estatísticas");

                var (active, inactive, admin, user) = await _userRepository.GetUserStatisticsAsync();
                var total = active + inactive;

                var statistics = new UserStatisticsDto
                {
                    StatusStats = new StatusStatisticsDto
                    {
                        Active = active,
                        Inactive = inactive,
                        ActivePercentage = total > 0 ? Math.Round((active * 100.0 / total), 1) : 0,
                        InactivePercentage = total > 0 ? Math.Round((inactive * 100.0 / total), 1) : 0
                    },
                    RoleStats = new RoleStatisticsDto
                    {
                        Admin = admin,
                        User = user,
                        AdminPercentage = total > 0 ? Math.Round((admin * 100.0 / total), 1) : 0,
                        UserPercentage = total > 0 ? Math.Round((user * 100.0 / total), 1) : 0
                    }
                };

                Console.WriteLine($"Estatísticas calculadas - Total: {total}");
                return statistics;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERRO no GetUserStatisticsAsync: {ex.Message}");
                throw;
            }
        }
    }

}