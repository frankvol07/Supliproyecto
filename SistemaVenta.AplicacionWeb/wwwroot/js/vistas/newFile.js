$(("#btnNuevo").click(function() {
    mostrarModal();
},

    $("#btnGuardar").click(function() {
        // Validar que los campos no estén vacíos
        /*debugger;*/
        const inputs = $("input.input-validar").serializeArray();
        const inputs_sin_valor = inputs.filter((item) => item.value.trim() == "");
        if (inputs_sin_valor.length > 0) {
            const mensaje = `Debe completar el campo:"${inputs_sin_valor[0].name}"`;
            toastr.warning("", mensaje);
            $(`input[name="${inputs_sin_valor[0].name}"]`).focus();
            return;
        }

        const modelo = struturedClone(MODELO_BASE);
        modelo["idUsuario"] = parseInt($("#txtId").val());
        modelo["nombre"] = $("#txtNombre").val();
        modelo["correo"] = $("#txtCorreo").val();
        modelo["telefono"] = $("#txtTelefono").val();
        modelo["idRol"] = $("cboRol").val();
        modelo["esActivo"] = $("cboEstado").val();

        const inputFoto = document.getElementById("txtFoto");

        const formData = new formData();

        formData.append("foto", inputFoto.files[0]);
        formData.append("modelo", JSON.stringify(modelo));

        $("#modalData").find("div.modal-content").LoadingOverlay("show");

        if (modelo.idUsuario == 0) {
            fetch("/Usuario/Crear", {
                method: "POST",
                body: formData
            })
                .then(response => {
                    $("#modalData").find("div.modal-content").LoadingOverlay("hide");
                    return response.ok ? response.json() : Promise.reject(response);
                })

                .then(reponseJson => {
                    if (resposeJson.estado) {

                        tablaData.row.add(resposeJson.objeto).draw(false);
                        $("#modalData").modal("hide");
                        swal("Listo!", "El usuario fue creado", "success");
                    } else {
                        swal("Lo sentimos!", responseJson.mensaje, "error");
                    }

                });

        } else {
            fetch("/Usuario/Editar", {
                method: "PUT",
                body: formData
            })
                .then(response => {
                    $("#modalData").find("div.modal-content").LoadingOverlay("hide");
                    return response.ok ? response.json() : Promise.reject(response);
                })

                .then(reponseJson => {
                    if (resposeJson.estado) {

                        tablaData.row(filaSeleccionada).data(responseJson.objeto).draw(false);
                        filaSeleccionada = null;
                        $("#modalData").modal("hide");
                        swal("Listo!", "El usuario fue modificado", "success");
                    } else {
                        swal("Lo sentimos!", responseJson.mensaje, "error");
                    }

                });

        }

    }),

    let, filaSeleccionada));
