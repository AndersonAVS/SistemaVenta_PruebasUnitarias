
//logica para mostrar los elementos ya que si se quiere buscar por fechas se tendría que ocultar el txt de búsqueda por numero de venta y viceversa
const VISTA_BUSQUEDA = {

    busquedaFecha: () => {

        $("#txtFechaInicio").val("")
        $("#txtFechaFin").val("")
        $("#txtNumeroVenta").val("")

        $(".busqueda-fecha").show() // . permite utilizar el nombre de una clase
        $(".busqueda-venta").hide()
    }, busquedaVenta: () => {

        $("#txtFechaInicio").val("")
        $("#txtFechaFin").val("")
        $("#txtNumeroVenta").val("")

        $(".busqueda-fecha").hide()
        $(".busqueda-venta").show()
    }
}

//el evento de ready para saber cuando nuestro documento ya esta cargado
$(document).ready(function () {
    VISTA_BUSQUEDA["busquedaFecha"]() //se llama al tipo de busqueda

    $.datepicker.setDefaults($.datepicker.regional["es"]) //idioma en el que se muestra el calendario

    //se configura las cajas de texto en ese formato del calendario
    $("#txtFechaInicio").datepicker({dateFormat : "dd/mm/yy"})
    $("#txtFechaFin").datepicker({ dateFormat: "dd/mm/yy" })

})

//el evento change de desplegable cboBuscarPor
$("#cboBuscarPor").change(function () {

    if ($("#cboBuscarPor").val() == "fecha") {
        VISTA_BUSQUEDA["busquedaFecha"]()
    } else {
        VISTA_BUSQUEDA["busquedaVenta"]()
    }

})


$("#btnBuscar").click(function () {

    if ($("#cboBuscarPor").val() == "fecha") { //validar que se seleccionó (fecha o NumeroVenta)
        //si es fecha
        if ($("#txtFechaInicio").val().trim() == "" || $("#txtFechaFin").val().trim() == "") { //que no estén vacías
            toastr.warning("", "Debe ingresar una fecha de inicio y fecha de fin")
            return;
        }
    } else {

        if ($("#txtNumeroVenta").val().trim() == "") {
            toastr.warning("", "Debe ingresar el Número de venta")
            return;
        }
    }

    //se tienen que obtener los valores de estos campos
    let numeroVenta = $("#txtNumeroVenta").val()
    let fechaInicio = $("#txtFechaInicio").val()
    let fechaFin = $("#txtFechaFin").val()

    //es la misma lógica de Negocio_Index
    $(".card-body").find("div.row").LoadingOverlay("show");

    fetch(`/Venta/Historial?numeroVenta=${numeroVenta}&fechaInicio=${fechaInicio}&fechaFin=${fechaFin}`)
        .then(response => {
            $(".card-body").find("div.row").LoadingOverlay("hide");
            return response.ok ? response.json() : Promise.reject(response);
        })
        .then(responseJson => {

            $("#tbventa tbody").html(""); //se limpia el tbody para agregar los elementos

            if (responseJson.length > 0) { //validar la longitud que contiene ya que va a ser una lista y la lista tiene qu ser mayor a 0

                responseJson.forEach((venta) => {

                    $("#tbventa tbody").append(
                        $("<tr>").append(
                            $("<td>").text(venta.fechaRegistro),
                            $("<td>").text(venta.numeroVenta),
                            $("<td>").text(venta.tipoDocumentoVenta),
                            $("<td>").text(venta.documentoCliente),
                            $("<td>").text(venta.nombreCliente),
                            $("<td>").text(venta.total),
                            $("<td>").append(
                                $("<button>").addClass("btn btn-info btn-sm").append(
                                    $("<i>").addClass("fas fa-eye")
                                ).data("venta", venta)
                            )
                        )
                    )

                })

            }

        })

})

//la siguiente lógica es al darle click al botón que va a tener cada venta este nos muestre un modal que va a contener todo el detalle de la venta
$("#tbventa tbody").on("click", ".btn-info", function () {

    let d = $(this).data("venta") //variable para hacer uso de este boton y se accede a la propiedad data y por medio de esta variable se obtiene la informacion

    //todos los campos que necesitamos para pintar la información
    $("#txtFechaRegistro").val(d.fechaRegistro)
    $("#txtNumVenta").val(d.numeroVenta)
    $("#txtUsuarioRegistro").val(d.usuario)
    $("#txtTipoDocumento").val(d.tipoDocumentoVenta)
    $("#txtDocumentoCliente").val(d.documentoCliente)
    $("#txtNombreCliente").val(d.nombreCliente)
    $("#txtSubTotal").val(d.subTotal)
    $("#txtIGV").val(d.impuestoTotal)
    $("#txtTotal").val(d.total)


    $("#tbProductos tbody").html(""); //para limipar el body de una tabla

    d.detalleVenta.forEach((item) => { //se recorren los elementos y creamos la fila y las columnas

        $("#tbProductos tbody").append(
            $("<tr>").append(
                $("<td>").text(item.descripcionProducto),
                $("<td>").text(item.cantidad),
                $("<td>").text(item.precio),
                $("<td>").text(item.total),
            )
        )

    })

    //se configura la url del botón que es "linkImprimir"
    $("#linkImprimir").attr("href",`/Venta/MostrarPDFVenta?numeroVenta=${d.numeroVenta}`)

    $("#modalData").modal("show"); //para mostrar el modal de detalleVenta

})