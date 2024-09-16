
let ValorImpuesto = 0;
$(document).ready(function () { //obtener la lista de tipo documento para la venta

    //    /controlador/método
    fetch("/Venta/ListaTipoDocumentoVenta") //para obtener el desplegable de tipo de documento
        .then(response => {
            return response.ok ? response.json() : Promise.reject(response);
        })
        .then(responseJson => {
            if (responseJson.length > 0) {
                responseJson.forEach((item) => {
                    $("#cboTipoDocumentoVenta").append(
                        $("<option>").val(item.idTipoDocumentoVenta).text(item.descripcion)
                    )
                })
            }
        })


        //para la sección de detalle
    fetch("/Negocio/Obtener")
        .then(response => {
            return response.ok ? response.json() : Promise.reject(response);
        })
        .then(responseJson => {

            if (responseJson.estado) {

                const d = responseJson.objeto;

                console.log(d)

                $("#inputGroupSubTotal").text(`Sub total: ${d.simboloMoneda}`)
                $("#inputGroupIGV").text(`IGV(${d.porcentajeImpuesto}%): ${d.simboloMoneda}`)
                $("#inputGroupTotal").text(`Total: ${d.simboloMoneda}`)

                ValorImpuesto = parseFloat(d.porcentajeImpuesto) //para guardar el porcentaje de impuesto de modo que se puedan hacer operaciones
            }

        })
        //para la sección de productos ya que debemos digitar un producto ya sea por cualquier atributo y debería mostarse una lista que hace referencia a lo que se ha escrito como búsqueda
        //se lo trabajó con select2 que es una librería op, que en la lista nos muestra la imagen del producto y sus detalles
    $("#cboBuscarProducto").select2({
        ajax: {
            url: "/Venta/ObtenerProductos", //de donde se va a obtener la informacion, ya que en método ObtenerProductos como parámetro tiene una búsqueda y nos va adelvoer una lista de productos en base a esta búsqueda
            dataType: 'json', //el tipo de datos
            contentType: "application/json; charset=utf-8", //el tipo de contenido
            delay: 250, //tiempo de búsqueda que se va a tomar para realizar la busqueda
            data: function (params) {
                return {
                    busqueda: params.term
                };
            },
            processResults: function (data,) { //todo el proceso de los resultados es decir q va hacer cuando obtenga todos los resutalados

                return { //aquí se especifica la forma en la que se van a retornar nuestro resutlados
                    results: data.map((item) => (
                        {
                            id: item.idProducto,
                            text: item.descripcion,

                            marca: item.marca,
                            categoria : item.nombreCategoria,
                            urlImagen: item.urlImagen,
                            precio : parseFloat(item.precio)
                        }
                    ))
                };
            }
        },
        language: "es", //para el idioma
        placeholder: 'Buscar Producto...', //nombre del txt
        minimumInputLength: 1, //apartir de 1 caracter
        templateResult: formatoResultados //la plantilla en como se va a mostrar la informacion formatoResultados
    });



})

function formatoResultados(data) { //plantilla de como mostrar los resultados

    //esto es por defecto, ya que muestra el "buscando..."
    if (data.loading)
        return data.text;

    var contenedor = $(
        `<table width="100%">
            <tr>
                <td style="width:60px">
                    <img style="height:60px;width:60px;margin-right:10px" src="${data.urlImagen}"/>
                </td>
                <td>
                    <p style="font-weight: bolder;margin:2px">${data.marca}</p>
                    <p style="margin:2px">${data.text}</p>
                </td>
            </tr>
         </table>`
    );

    return contenedor;
}


//para que el cursor haga focus dentro de la caka de texto de búsqueda
$(document).on("select2:open", function () {
    document.querySelector(".select2-search__field").focus();
})

let ProductosParaVenta = [];
//Ahora al momento en que se encuentre al producto buscado vamos a hacer uso de un modal para indicar la cantidad de ese producto
$("#cboBuscarProducto").on("select2:select", function (e) {
    const data = e.params.data; //toda la data que contiene toda la info del producto

    //validar que el producto que se ha seleccionado no exista en losproductos que ya han sido registrados para la compra
    let producto_encontrado = ProductosParaVenta.filter(p => p.idProducto == data.id)
    if (producto_encontrado.length > 0) { //quiere decir que si ha encontrado un producto
        $("#cboBuscarProducto").val("").trigger("change") //en caso de que el producto ya exista se limpia la selección
        toastr.warning("", "Ya se agregregó el producto")
        return false
    }

    swal({
        title: data.marca,
        text: data.text,
        imageUrl: data.urlImagen,
        type:"input",
        showCancelButton: true,
        closeOnConfirm: false,
        inputPlaceholder: "Digite la cantidad"
    },
        function (valor) {

            if (valor === false) return false;

            if (valor === "") {
                toastr.warning("", "Necesita ingresar la cantidad")
                return false;
            }
            if (isNaN(parseInt(valor))) {
                toastr.warning("", "Debe ingresar un valor númerico")
                return false;
            }

            let producto = {
                idProducto: data.id,
                marcaProducto: data.marca,
                descripcionProducto: data.text,
                categoriaProducto: data.categoria,
                cantidad: parseInt(valor),
                precio: data.precio.toString(),
                total: (parseFloat(valor) * data.precio).toString()

            }

            ProductosParaVenta.push(producto) //dentro del array ponemos tods los atributos del producto

            mostrarProducto_Precios(); //método para mostrar los precios del producto
            $("#cboBuscarProducto").val("").trigger("change")
            swal.close()
        }
    )

})

function mostrarProducto_Precios() {

    let total = 0;
    let igv = 0;
    let subtotal = 0;
    let porcentaje = ValorImpuesto / 100;

    //se limpia la tabla del producto en caso tenga algún elemento
    $("#tbProducto tbody").html("")

    ProductosParaVenta.forEach((item) => { //este es el array que contiene todos los productos que se han seleccionado

        total = total + parseFloat(item.total)

        $("#tbProducto tbody").append(
            $("<tr>").append(
                $("<td>").append(
                    $("<button>").addClass("btn btn-danger btn-eliminar btn-sm").append( //botón para eliminar el producto de la lsita
                        $("<i>").addClass("fas fa-trash-alt")
                    ).data("idProducto",item.idProducto)
                ),
                $("<td>").text(item.descripcionProducto),
                $("<td>").text(item.cantidad),
                $("<td>").text(item.precio),
                $("<td>").text(item.total)
            )
        )
    })

    //Lógica para poder mostrar los precios
    subtotal = total / (1 + porcentaje);
    igv = total - subtotal;

    $("#txtSubTotal").val(subtotal.toFixed(2)) //con 2 decimales
    $("#txtIGV").val(igv.toFixed(2))
    $("#txtTotal").val(total.toFixed(2))


}

//esta es la lógica para poder eliminar un producto de nustra lista de productos seleccionados
$(document).on("click", "button.btn-eliminar", function () {

    const _idproducto = $(this).data("idProducto")

    ProductosParaVenta = ProductosParaVenta.filter(p => p.idProducto != _idproducto);

    mostrarProducto_Precios(); //actualiza la tabla para los precios
})

//para enviar toda la información de Detalle a nuestra tabla para que pueda registrar la venta
$("#btnTerminarVenta").click(function () {

    if (ProductosParaVenta.length < 1) { //validar q si es menor a 1 entonces quiere decir que no hay productos
        toastr.warning("", "Ningún producto ha sido ingresado")
        return;
    }

    const vmDetalleVenta = ProductosParaVenta;

    const venta = {
        //todo esto es una representación del VMVenta
        idTipoDocumentoVenta: $("#cboTipoDocumentoVenta").val(),
        documentoCliente: $("#txtDocumentoCliente").val(),
        nombreCliente: $("#txtNombreCliente").val(),
        subTotal: $("#txtSubTotal").val(),
        impuestoTotal: $("#txtIGV").val(),
        total: $("#txtTotal").val(),
        DetalleVenta : vmDetalleVenta //se le pasa el detalle de la venta
    }

    $("#btnTerminarVenta").LoadingOverlay("show");

    //Lógica de envio
    fetch("/Venta/RegistrarVenta", {
        method: "POST",
        headers: { "Content-Type": "application/json; charset=utf-8" },
        body: JSON.stringify(venta)
    })
        .then(response => {
            $("#btnTerminarVenta").LoadingOverlay("hide");
            return response.ok ? response.json() : Promise.reject(response);
        })
        .then(responseJson => {

            if (responseJson.estado) {
                ProductosParaVenta = []; //se limpia el array
                mostrarProducto_Precios(); //se ejecuta es te método

                //Para limpiar la informacion del cliente
                $("#txtDocumentoCliente").val("")
                $("#txtNombreCliente").val("")
                $("#cboTipoDocumentoVenta").val($("#cboTipoDocumentoVenta option:first").val())

                swal("Venta registrada", `Numero de venta: ${responseJson.objeto.numeroVenta}`, "success")
            } else {
                swal("Hubo un problema...", "No se pudo registrar la venta", "error")
            }
        })

})