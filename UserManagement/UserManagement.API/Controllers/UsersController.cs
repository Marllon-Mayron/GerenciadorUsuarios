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

        // Apenas ADMIN pode ver todos os usuários
        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        // ADMIN vê qualquer usuário, USER vê apenas o próprio perfil
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
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
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] CreateUserDto createUserDto)
        {
            try
            {
                var user = await _userService.CreateAsync(createUserDto);
                return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ADMIN pode atualizar qualquer usuário, USER apenas o próprio
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto updateUserDto)
        {
            try
            {
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
                return BadRequest(new { message = ex.Message });
            }
        }

        // Deletar usuários
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _userService.DeleteAsync(id);
                if (!result)
                    return NotFound(new { message = "Usuário não encontrado" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Metodo para mudar status
        [HttpPatch("{id}/status")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusDto changeStatusDto)
        {
            try
            {
                var user = await _userService.ChangeStatusAsync(id, changeStatusDto.Activate);
                return Ok(user);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Usuário não encontrado" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        //Promover usuários
        [HttpPost("{id}/promote")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> PromoteToAdmin(Guid id)
        {
            try
            {
                var adminUserId = GetCurrentUserId();
                var user = await _userService.PromoteToAdminAsync(id, adminUserId);
                return Ok(user);
            }
            catch (UnauthorizedAccessException ex)
            {
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
                return BadRequest(new { message = ex.Message });
            }
        }

        // Método para rebaixar admins
        [HttpPost("{id}/demote")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DemoteToUser(Guid id)
        {
            try
            {
                var adminUserId = GetCurrentUserId();
                var user = await _userService.DemoteToUserAsync(id, adminUserId);
                return Ok(user);
            }
            catch (UnauthorizedAccessException ex)
            {
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
                return BadRequest(new { message = ex.Message });
            }
        }

        // Método auxiliar para pegar ID do usuário atual
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("Usuário não autenticado");

            return Guid.Parse(userIdClaim);
        }
    }
}