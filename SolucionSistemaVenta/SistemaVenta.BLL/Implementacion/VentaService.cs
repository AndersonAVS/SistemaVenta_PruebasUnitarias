using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using SistemaVenta.BLL.Interfaces;
using SistemaVenta.DAL.Interfaces;
using SistemaVenta.Entity;

namespace SistemaVenta.BLL.Implementacion
{
    public class VentaService : IVentaService
    {
        private readonly IGenericRepository<Producto> _repositorioProducto;
        private readonly IVentaRepository _repositorioVenta;

        public VentaService(IGenericRepository<Producto> repositorioProducto,
            IVentaRepository repositorioVenta
            )
        {
            _repositorioProducto = repositorioProducto;
            _repositorioVenta = repositorioVenta;
        }
        

        public async Task<List<Producto>> ObtenerProductos(string busqueda)
        {
            // Declara una consulta que se ejecutará de forma asincrónica sobre el repositorio de productos.
            IQueryable<Producto> query = await _repositorioProducto.Consultar(p =>
                // Filtra los productos que están activos (EsActivo == true),
                p.EsActivo == true &&
                // Tienen stock disponible (Stock > 0),
                p.Stock > 0 &&
                // Y cuya concatenación de CodigoBarra, Marca y Descripcion contiene el término de búsqueda proporcionado.
                string.Concat(p.CodigoBarra, p.Marca, p.Descripcion).Contains(busqueda)
                //En el formulario de venta vamos a tener que escribir ya sea el código de barra, la marca o
                //la descripcion del producto cualquiera de estos campos y lo vamos a digitar en una caja de
                //texto y deacuerdo a esta busqueda nosotros debemos obtener todos los productos relacionados
                //con esta busqueda
            );

            // Devuelve la lista de productos que cumplen con los criterios anteriores,
            // incluyendo también la navegación a la entidad relacionada 'Categoria' (IdCategoriaNavigation).
            return query.Include(c => c.IdCategoriaNavigation).ToList();
        }



        public async Task<Venta> Registrar(Venta entidad)
        {
            try
            {
                //Aquí lo unico que se hace es ejecutar el metodo registrar  que ya tenemos en nuestro repositorio
                return await _repositorioVenta.Registrar(entidad);
            }
            catch {
                throw;
            }
        }


        public async Task<List<Venta>> Historial(string numeroVenta, string fechaInicio, string fechaFin)
        {
            // Declara una consulta que se ejecutará de forma asincrónica sobre el repositorio de ventas.
            IQueryable<Venta> query = await _repositorioVenta.Consultar();

            // Asigna cadenas vacías a fechaInicio y fechaFin si son nulas.
            fechaInicio = fechaInicio is null ? "" : fechaInicio;
            fechaFin = fechaFin is null ? "" : fechaFin;

            // Si ambas fechas están proporcionadas (no son cadenas vacías),
            if (fechaInicio != "" && fechaFin != "")
            {
                // Convierte las cadenas de fecha proporcionadas a objetos DateTime, utilizando el formato de fecha "dd/MM/yyyy" y la cultura "es-PE".
                DateTime fech_inicio = DateTime.ParseExact(fechaInicio, "dd/MM/yyyy", new CultureInfo("es-PE"));
                DateTime fech_fin = DateTime.ParseExact(fechaFin, "dd/MM/yyyy", new CultureInfo("es-PE"));

                // Filtra la consulta de ventas para incluir solo aquellas registradas entre las fechas proporcionadas (inclusive)
                return query.Where(v =>
                    v.FechaRegistro.Value.Date >= fech_inicio.Date &&
                    v.FechaRegistro.Value.Date <= fech_fin.Date
                )
                    // Incluye las entidades relacionadas en los resultados para evitar problemas de carga diferida (lazy loading)
                    .Include(tdv => tdv.IdTipoDocumentoVentaNavigation) // Incluye la navegación de tipo de documento de venta
                    .Include(u => u.IdUsuarioNavigation) // Incluye la navegación del usuario
                    .Include(dv => dv.DetalleVenta) // Incluye los detalles de la venta
                    .ToList(); // Nos devuelve toda la lista
            } // Todo esto es para el caso en que estemos trabajando con fechas
            else // En caso de que no se esté trabajando con fechas sino que estemos obteniendo el historial de una venta según su número de venta necesitamos aplicar otro tipo de validaciones
            {
                // Si no se proporcionaron ambas fechas, filtra la consulta para incluir solo la venta con el número de venta proporcionado.
                return query.Where(v => v.NumeroVenta == numeroVenta
                   )
                    // Incluye las entidades relacionadas en los resultados para evitar problemas de carga diferida (lazy loading)
                    .Include(tdv => tdv.IdTipoDocumentoVentaNavigation) // Incluye la navegación de tipo de documento de venta.
                    .Include(u => u.IdUsuarioNavigation) // Incluye la navegación del usuario
                    .Include(dv => dv.DetalleVenta) // Incluye los detalles de la venta
                    .ToList(); // Ejecuta la consulta y convierte los resultados en una lista
            }
        }

        //Con el siguiente método vamos a obtener el detalle de solo una venta en específico, no va a ser como historial que puede ser por una o por un rango de fechas
        public async Task<Venta> Detalle(string numeroVenta)
        {
            IQueryable<Venta> query = await _repositorioVenta.Consultar(v => v.NumeroVenta == numeroVenta);

           return query
                       .Include(tdv => tdv.IdTipoDocumentoVentaNavigation)
                       .Include(u => u.IdUsuarioNavigation)
                       .Include(dv => dv.DetalleVenta)
                       .First(); //Es igual a lo demás pero esta vez es devolver al primero que encuentre
        }

        //En este método Reporte si o si necesitamos recibir una fehca de inicio y tambien una fecha de fin
        //La lógica es parecida a la del Historial
        public async Task<List<DetalleVenta>> Reporte(string fechaInicio, string fechaFin)
        {
            DateTime fech_inicio = DateTime.ParseExact(fechaInicio, "dd/MM/yyyy", new CultureInfo("es-PE"));
            DateTime fech_fin = DateTime.ParseExact(fechaFin, "dd/MM/yyyy", new CultureInfo("es-PE"));


            List<DetalleVenta> lista = await _repositorioVenta.Reporte(fech_inicio, fech_fin);

            return lista; //Se retorna la lista de este método
        }
    }
}

//Lueo se tiene que usar estas dependencias dentro de la capa IOC
