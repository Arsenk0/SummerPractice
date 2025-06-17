// TransportLogistics.Api/Controllers/OrderController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TransportLogistics.Api.Contracts;
using TransportLogistics.Api.DTOs;
using TransportLogistics.Api.DTOs.QueryParams; // Додано для OrderQueryParams
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TransportLogistics.Api.Controllers
{
    [Authorize] // Додано авторизацію для всього контролера
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // Оновлено метод для прийому OrderQueryParams
        [HttpGet]
        [ProducesResponseType(typeof(List<OrderResponse>), 200)]
        public async Task<IActionResult> GetAllOrders([FromQuery] OrderQueryParams queryParams)
        {
            var orders = await _orderService.GetAllOrdersAsync(queryParams);
            return Ok(orders);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(OrderResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound($"Order with ID {id} not found.");
            }
            return Ok(order);
        }

        [HttpPost]
        [ProducesResponseType(typeof(OrderResponse), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                var newOrder = await _orderService.CreateOrderAsync(request);
                // Повертаємо 201 Created та посилання на створений ресурс
                return CreatedAtAction(nameof(GetOrderById), new { id = newOrder.Id }, newOrder);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(OrderResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateOrder(Guid id, [FromBody] UpdateOrderRequest request)
        {
            try
            {
                var updatedOrder = await _orderService.UpdateOrderAsync(id, request);
                if (updatedOrder == null)
                {
                    return NotFound($"Order with ID {id} not found.");
                }
                return Ok(updatedOrder);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteOrder(Guid id)
        {
            var deleted = await _orderService.DeleteOrderAsync(id);
            if (!deleted)
            {
                return NotFound($"Order with ID {id} not found.");
            }
            return NoContent(); // 204 No Content for successful deletion
        }
    }
}
