using Microsoft.AspNetCore.Mvc;

//a{adiendo referencia necesarias
using AutoMapper;
using Newtonsoft.Json;
using SistemaVenta.AplicacionWeb.Models.ViewModels;
using SistemaVenta.AplicacionWeb.Utilidades.Response;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.Entity;
using Microsoft.AspNetCore.Authorization;

namespace SistemaVenta.AplicacionWeb.Controllers
{
    [Authorize]
    public class NegocioController : Controller
    {

        private readonly IMapper _mapper;
        private readonly INegocioService _negocioService;

        public NegocioController(IMapper mapper, INegocioService negocioService)
        {
            _mapper = mapper;
            _negocioService = negocioService;
        }

        public IActionResult Index()
        {
            return View();
        }


        //Este método lo único que va a hacer es devolver toda la información del negocio
        [HttpGet]
        public async Task<IActionResult> Obtener()
        {
            GenericResponse<VMNegocio> gResponse = new GenericResponse<VMNegocio>(); //para devlver cualquier tipo de respuesta pero con un formato VMNegicio

            try
            {
                VMNegocio vmNegocio = _mapper.Map<VMNegocio>(await _negocioService.Obtener()); //Obtener es el metodo q esta devolviendo toda la informacion del negocio pero lo esta devolviendo en una clase negocio y lo estamos convirtiendo en una clas VMNegocio

                gResponse.Estado = true;
                gResponse.Objeto = vmNegocio;
            }
            catch(Exception ex) {
                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;
            }

            //devolver la respuesta a traves de un statuscodde (StatusCodes.Status200OK) es par auna respuesta exitosa
            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        //
        [HttpPost]
        public async Task<IActionResult> GuardarCambios([FromForm]IFormFile logo, [FromForm]string modelo)
        {
            GenericResponse<VMNegocio> gResponse = new GenericResponse<VMNegocio>();

            try
            {
                VMNegocio vmNegocio = JsonConvert.DeserializeObject<VMNegocio>(modelo); //convertir el string modelo

                string nombreLogo = "";
                Stream logoStream = null;

                //logica para validar que sea diferente de nulo
                if (logo != null) { 
                
                    string nombre_en_codigo = Guid.NewGuid().ToString("N"); //número y letras
                    string extension = Path.GetExtension(logo.FileName); //necesitamos la extension de este archivo
                    nombreLogo = string.Concat(nombre_en_codigo, extension); //crear el nombre del logo con la extension
                    logoStream = logo.OpenReadStream(); //se le pasa el valor que contiene logo
                }

                //vamos a enviarle nuestro negocio 
                //lo que va a obtener meocio_editado es toda la configuracion de arriba a nuestro servicio para poder guardar los cambios
                Negocio negocio_editado = await _negocioService.GuardarCambios(_mapper.Map<Negocio>(vmNegocio)
                    , logoStream, nombreLogo);
                //el negocio_editado va a almacenar todos los cambios que se hab realizado en el modelo pasandole la imagen y el nombre del logo

                //aca lo q se hace es converir nuevamente a vmnegocio el negocio_edita que se esta recibiendo
                vmNegocio = _mapper.Map<VMNegocio>(negocio_editado);

                gResponse.Estado = true;
                gResponse.Objeto = vmNegocio;
            }
            catch (Exception ex)
            {
                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;
            }


            return StatusCode(StatusCodes.Status200OK, gResponse); //se le devulve es estandar GenericResponse
        }



    }
}
//Luego se tiene qu crear una hoja de js par anuestra vista en la cual vamos a estar implementando estos métodos