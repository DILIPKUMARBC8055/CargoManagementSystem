
using CargoManagement.Api.Responses;
using CargoManagement.DTOs;
using CargoManagementProject.core.Services;
using CargoManagementProject.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CargoManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]

    public class CargoController : ControllerBase
    {
        private readonly ICargoService _cargoService;

        public CargoController(ICargoService cargoService)
        {
            _cargoService = cargoService;
        }

        /// <summary>
        /// Retrieves all cargos.
        /// </summary>
        /// <returns>A list of cargos.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllCargos()
        {
            var cargos = await _cargoService.GetAllCargosAsync();
            return Ok(new ApiResponse<IEnumerable<Cargo>>
            {
                Success = true,
                Data = cargos
            });
        }

        /// <summary>
        /// Retrieves a specific cargo by ID.
        /// </summary>
        /// <param name="id">The ID of the cargo.</param>
        /// <returns>The requested cargo.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCargoById(int id)
        {
            var cargo = await _cargoService.GetCargoByIdAsync(id);
            if (cargo == null)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = "Cargo not found."
                });
            }

            return Ok(new ApiResponse<Cargo>
            {
                Success = true,
                Data = cargo
            });
        }

        /// <summary>
        /// Creates a new cargo.
        /// </summary>
        /// <param name="cargo">The cargo entity to create.</param>
        /// <returns>A success message.</returns>
        [HttpPost]
        public async Task<IActionResult> AddCargo([FromBody] Cargo cargo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Invalid cargo data."
                });
            }

            await _cargoService.AddCargoAsync(cargo);
            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Cargo added successfully."
            });
        }

        /// <summary>
        /// Updates an existing cargo.
        /// </summary>
        /// <param name="id">The ID of the cargo to update.</param>
        /// <param name="cargo">The updated cargo entity.</param>
        /// <returns>A success message.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCargo(int id, [FromBody] Cargo cargo)
        {
            if (id != cargo.Id)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Cargo ID mismatch."
                });
            }

            var existingCargo = await _cargoService.GetCargoByIdAsync(id);
            if (existingCargo == null)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = "Cargo not found."
                });
            }

            await _cargoService.UpdateCargoAsync(cargo);
            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Cargo updated successfully."
            });
        }

        /// <summary>
        /// Deletes a cargo by ID.
        /// </summary>
        /// <param name="id">The ID of the cargo to delete.</param>
        /// <returns>A success message.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCargo(int id)
        {
            var existingCargo = await _cargoService.GetCargoByIdAsync(id);
            if (existingCargo == null)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = "Cargo not found."
                });
            }

            await _cargoService.DeleteCargoAsync(id);
            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Cargo deleted successfully."
            });
        }
    }

}
