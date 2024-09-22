using Flyurdreamapi.Authorize;
using Flyurdreamcommands.Models.Datafields;
using Flyurdreamcommands.Repositories.Abstract;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Flyurdreamapi.Controllers
{
    [Route("api")]
    [ApiController]
    [EnableCors("cors")]
  //  [Auth]
    public class MenuController : ControllerBase
    {
        public readonly IMenuRepository _MenuRepository;
        public MenuController(IMenuRepository menuRepository)
        {
            _MenuRepository = menuRepository;
        }
        [HttpGet("/menu")]
        
        public ActionResult<List<MenuItem>> GetMenu()
        {
            List<MenuItem> menuItems = _MenuRepository.GetMenuItems();
            return menuItems;
        }
        [HttpGet("/GetMenu")]
        public async Task<ActionResult<List<MenuItem>>> GetMenuBasedonGroup(int group)
        {
            List<MenuItem> menuItems = await _MenuRepository.GetMenuBasedonGroup(group);
            return menuItems;
        }
    }
}
