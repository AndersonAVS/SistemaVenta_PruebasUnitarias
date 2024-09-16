using Microsoft.AspNetCore.Mvc;

using AutoMapper;
using SistemaVenta.AplicacionWeb.Models.ViewModels;
using SistemaVenta.AplicacionWeb.Utilidades.Response;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.Entity;

//referencias para generar el pdf
using DinkToPdf;
using DinkToPdf.Contracts;

using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SistemaVenta.AplicacionWeb.Controllers
{
    [Authorize]
    public class VentaController : Controller
    {
        private readonly ITipoDocumentoVentaService _tipoDocumentoVentaServicio;
        private readonly IVentaService _ventaServicio;
        private readonly IMapper _mapper;
        private readonly IConverter _converter;

        public VentaController(ITipoDocumentoVentaService tipoDocumentoVentaServicio,
            IVentaService ventaServicio,
            IMapper mapper,
             IConverter converter
            )
        {
            _tipoDocumentoVentaServicio = tipoDocumentoVentaServicio;
            _ventaServicio = ventaServicio;
            _mapper = mapper;
            _converter = converter;
        }

        public IActionResult NuevaVenta()
        {
            return View();
        }

        public IActionResult HistorialVenta()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ListaTipoDocumentoVenta()
        {
            //Obtiene una lista de tipos de documentos de venta a través del servicio _tipoDocumentoVentaServicio de forma asincrónica.
            List<VMTipoDocumentoVenta> vmListaTipoDocumentos = _mapper.Map<List<VMTipoDocumentoVenta>>(await _tipoDocumentoVentaServicio.Lista());

            //Devuelve la lista de tipos de documentos de venta con un código de estado HTTP 200 (OK).
            return StatusCode(StatusCodes.Status200OK, vmListaTipoDocumentos);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerProductos(string busqueda)
        {
            List<VMProducto> vmListaProductos = _mapper.Map<List<VMProducto>>(await _ventaServicio.ObtenerProductos(busqueda));

            return StatusCode(StatusCodes.Status200OK, vmListaProductos);
        }



        [HttpPost]
        public async Task<IActionResult> RegistrarVenta([FromBody] VMVenta modelo) //El parámetro se obtiene desde el cuerpo que se llama FromBody
        {

            GenericResponse<VMVenta> gResponse = new GenericResponse<VMVenta>();

            try
            {
                ////Com este metodo se va a encargar de registrar una venta entonces debemos pasarle el usuario de quien esta reaizando esta venta
                //ClaimsPrincipal claimUser = HttpContext.User;

                //string idUsuario = claimUser.Claims
                //    .Where(c => c.Type == ClaimTypes.NameIdentifier)
                //    .Select(c => c.Value).SingleOrDefault();

                //modelo.IdUsuario = int.Parse(idUsuario);
                modelo.IdUsuario = 1; //esto es temporal ya que tendría que ser deacuerdo al rol del usuario

                Venta venta_creada = await _ventaServicio.Registrar(_mapper.Map<Venta>(modelo));
                modelo = _mapper.Map<VMVenta>(venta_creada);

                gResponse.Estado = true;
                gResponse.Objeto = modelo;

            }
            catch(Exception ex) {

                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }



        [HttpGet]
        public async Task<IActionResult> Historial(string numeroVenta,string fechaInicio, string fechaFin)
        {

            List<VMVenta> vmHistorialVenta = _mapper.Map<List<VMVenta>>(await _ventaServicio.Historial(numeroVenta, fechaInicio, fechaFin));

            return StatusCode(StatusCodes.Status200OK, vmHistorialVenta);
        }


        public IActionResult MostrarPDFVenta(string numeroVenta)
        {

            string urlPlantillaVista = $"{this.Request.Scheme}://{this.Request.Host}/Plantilla/PDFVenta?numeroVenta={numeroVenta}";

            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = new GlobalSettings()
                {
                    PaperSize = PaperKind.A4,
                    Orientation = Orientation.Portrait,
                },
                Objects = {
                    new ObjectSettings(){
                        Page = urlPlantillaVista
                    }
                }
            };

            var archivoPDF = _converter.Convert(pdf);

            return File(archivoPDF, "application/pdf");

        }

    }
}
