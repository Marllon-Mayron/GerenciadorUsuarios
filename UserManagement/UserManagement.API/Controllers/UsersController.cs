using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using UserManagement.Application.DTOs;
using UserManagement.Application.Services;
using UserManagement.Domain.Enums;

namespace UserManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        // ==================== 1. ROTAS FIXAS (SEM PARÂMETROS) ====================

        [HttpGet("paginator")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetAllPaginator([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                Console.WriteLine($"Buscando usuários - Página: {pageNumber}, Tamanho: {pageSize}");

                var (users, totalCount) = await _userService.GetAllPaginatedAsync(pageNumber, pageSize);

                Console.WriteLine($"Encontrados {users.Count()} de {totalCount} usuários");

                var response = new
                {
                    Items = users,
                    TotalItems = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                    HasPreviousPage = pageNumber > 1,
                    HasNextPage = pageNumber < (int)Math.Ceiling(totalCount / (double)pageSize)
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERRO no GetAllPaginator: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Erro interno ao processar a requisição", details = ex.Message });
            }
        }

        [HttpGet("statistics")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                Console.WriteLine("GET /api/users/statistics - Requisição recebida");

                var statistics = await _userService.GetUserStatisticsAsync();

                Console.WriteLine("Estatísticas obtidas com sucesso");
                return Ok(new
                {
                    success = true,
                    data = statistics,
                    message = "Estatísticas obtidas com sucesso"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERRO em GetStatistics: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");

                return StatusCode(500, new
                {
                    success = false,
                    message = "Erro interno ao processar a requisição",
                    error = ex.Message
                });
            }
        }

        // ==================== 2. ROTAS COM SUB-ROTAS (/{id}/ação) ====================

        [HttpPost("{id}/promote")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> PromoteToAdmin(Guid id)
        {
            try
            {
                Console.WriteLine($"=== PROMOVER PARA ADMIN ===");
                Console.WriteLine($"ID do usuário alvo: {id}");

                var adminUserId = GetCurrentUserId();
                Console.WriteLine($"ID do admin: {adminUserId}");

                var user = await _userService.PromoteToAdminAsync(id, adminUserId);
                return Ok(user);
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Unauthorized: {ex.Message}");
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro inesperado: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/demote")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DemoteToUser(Guid id)
        {
            try
            {
                Console.WriteLine($"=== REBAIXAR PARA USER ===");
                Console.WriteLine($"ID do usuário alvo: {id}");

                var adminUserId = GetCurrentUserId();
                Console.WriteLine($"ID do admin: {adminUserId}");

                var user = await _userService.DemoteToUserAsync(id, adminUserId);
                return Ok(user);
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Unauthorized: {ex.Message}");
                return Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro inesperado: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("{id}/status")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusDto changeStatusDto)
        {
            try
            {
                Console.WriteLine($"=== MUDAR STATUS ===");
                Console.WriteLine($"ID do usuário: {id}");
                Console.WriteLine($"Ativar: {changeStatusDto.Activate}");

                var user = await _userService.ChangeStatusAsync(id, changeStatusDto.Activate);
                return Ok(user);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Usuário não encontrado" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        // ==================== 3. ROTAS COM ID (/{id}) ====================

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                Console.WriteLine($"=== BUSCAR USUÁRIO POR ID ===");
                Console.WriteLine($"ID: {id}");

                var currentUserId = GetCurrentUserId();
                var isAdmin = User.HasClaim(c => c.Type == "Role" && c.Value == UserRole.Admin.ToString());

                // Se não for admin, só pode ver o próprio perfil
                if (!isAdmin && currentUserId != id)
                    return Forbid();

                var user = await _userService.GetByIdAsync(id);
                if (user == null)
                    return NotFound(new { message = "Usuário não encontrado" });

                return Ok(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto updateUserDto)
        {
            try
            {
                Console.WriteLine($"=== ATUALIZAR USUÁRIO ===");
                Console.WriteLine($"ID: {id}");

                var currentUserId = GetCurrentUserId();
                var isAdmin = User.IsInRole("Admin");

                if (!isAdmin && currentUserId != id)
                    return Forbid();

                var user = await _userService.UpdateAsync(id, updateUserDto);
                return Ok(user);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Usuário não encontrado" });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                Console.WriteLine($"=== DELETAR USUÁRIO ===");
                Console.WriteLine($"ID: {id}");

                var result = await _userService.DeleteAsync(id);
                if (!result)
                    return NotFound(new { message = "Usuário não encontrado" });

                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        // ==================== 4. ROTAS BASE (SEM PARÂMETROS ADICIONAIS) ====================

        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetAll()
        {
            Console.WriteLine("=== BUSCAR TODOS OS USUÁRIOS ===");
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] CreateUserDto createUserDto)
        {
            try
            {
                Console.WriteLine("=== CRIAR NOVO USUÁRIO ===");
                Console.WriteLine($"Email: {createUserDto.Email}");

                var user = await _userService.CreateAsync(createUserDto);
                return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        // ==================== 5. MÉTODOS AUXILIARES ====================

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("Usuário não autenticado");

            return Guid.Parse(userIdClaim);
        }
    }
}