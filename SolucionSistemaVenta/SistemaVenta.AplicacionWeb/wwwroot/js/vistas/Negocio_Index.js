/*esta hoja de js se tiene que llamar en la vista en Views/Negocio/Index*/
//hay que obtener todos los datos del negocio para poder mostrarlos dentro del formulario

//esta función lo que va a hacer es ejecutar la petición de esta api que se creó anteriormente para poder obtener toda la informacion
$(document).ready(function () {


    $(".card-body").LoadingOverlay("show"); //loading

    fetch("/Negocio/Obtener")
        .then(response => { // en el caso de que exista un error simplemente lo va a cancelar y en caso e q este todo correcto va a devolver un json
            $(".card-body").LoadingOverlay("hide"); //se oculta el loading
            return response.ok ? response.json() : Promise.reject(response);
        })
        .then(responseJson => {

            console.log(responseJson)

            if (responseJson.estado) {
                const d = responseJson.objeto

                $("#txtNumeroDocumento").val(d.numeroDocumento)
                $("#txtRazonSocial").val(d.nombre)
                $("#txtCorreo").val(d.correo)
                $("#txtDireccion").val(d.direccion)
                $("#txTelefono").val(d.telefono)
                $("#txtImpuesto").val(d.porcentajeImpuesto)
                $("#txtSimboloMoneda").val(d.simboloMoneda)
                $("#imgLogo").attr("src", d.urlLogo)


            } else {
                swal("Hubo un error...", responseJson.mensaje, "error")

            }
        })


})


$("#btnGuardarCambios").click(function () {

    const inputs = $("input.input-validar").serializeArray(); //se serealizan todos los elementos que tengan la clase input-validar, se lo almacena en inputs
    const inputs_sin_valor = inputs.filter((item) => item.value.trim() == "") //se valida a aquellos que su lvalor sea vacio y se lo guarda en inpus_sin_valor

    if (inputs_sin_valor.length > 0) { //recorrido en el caso de que la cantaidad length sea > 0
        const mensaje = `Debe completar el campo : "${inputs_sin_valor[0].name}"`; // que nos muestre un mensaje diciendo que necesita completar esta campo
        toastr.warning("", mensaje)
        $(`input[name="${inputs_sin_valor[0].name}"]`).focus()
        return;
    }
    //si todo está bien se procede con la creacion de un modelo y tienen que tener exactamente las propiedades de nuestra entidad
    const modelo = {
        numeroDocumento: $("#txtNumeroDocumento").val(),
        nombre: $("#txtRazonSocial").val(),
        correo: $("#txtCorreo").val(),
        direccion: $("#txtDireccion").val(),
        telefono: $("#txTelefono").val(),
        porcentajeImpuesto: $("#txtImpuesto").val(),
        simboloMoneda: $("#txtSimboloMoneda").val()
    }

    const inputLogo = document.getElementById("txtLogo")

    const formData = new FormData()

    formData.append("logo", inputLogo.files[0])
    formData.append("modelo",JSON.stringify(modelo))


    $(".card-body").LoadingOverlay("show");

 
    fetch("/Negocio/GuardarCambios", { //configurando la url
        method: "POST",
        body: formData
        })
        .then(response => {
            $(".card-body").LoadingOverlay("hide");
            return response.ok ? response.json() : Promise.reject(response);
        })
        .then(responseJson => {

            if (responseJson.estado) {
                const d = responseJson.objeto
                //necesitamos pintar la imagen y esa url de la imagen lo va a devolver ese objeto, entonces necesitamos acceder a la url que contiene ese objeto
                $("#imgLogo").attr("src",d.urlLogo)

            } else {
                swal("Hubo un problema...", responseJson.mensaje, "error")

            }
        })


})