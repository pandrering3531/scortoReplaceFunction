
//CONTROLADOR PARA LA OPCIÓN DE CONFIGURAR PERMISOS

//Create a Controller
// aquí $scope se utiliza para compartir datos entre vista y el controlador
registrationModule.controller('ObjectPermmisionsController', function ($scope, ObjectPermissionServices) {

    //Objeto lista que guarda los datos de la tabla ObjectPermissions
    $scope.data = null;

    $scope.idSelectedRow = null;


    /* configura dialog para editar linea */

    $scope.open = function () {
        $scope.showModal = true;


    };

    $scope.ok = function () {
        $scope.arrpermissions = [];
        $scope.arrpermissions = new Array();

        //Valida que se seleccione un rol
        if ($scope.selectedRole != undefined) {

            if ($scope.checkboxModel.value1 == true) {
                $scope.arrpermissions.push("R");
            }
            if ($scope.checkboxModel.value2 == true) {
                $scope.arrpermissions.push("U");
            }
            if ($scope.checkboxModel.value3 == true) {
                $scope.arrpermissions.push("D");
            }
            ObjectPermissionServices.SaveData($scope).then(function (d) {

                if (d == 'Success') {


                    //have to clear form here
                    ClearForm();
                    alert('Registro agregado con éxito');
                    ObjectPermissionServices.GetList($scope.selectedOption.Value).then(function (d) {
                        $scope.data = d.data;
                        $scope.showModal = false;

                    }, function (error) {
                        alert('Error!');
                    });
                }
                else {
                    alert(d);
                }
            });

        }
        else {
            alert("Seleccione un rol")
        }

    };


    function ClearForm() {
        ClearChecks();
        $scope.selectedOptionRoles = "0";

    }
    function ClearChecks() {
        $scope.checkboxModel.value1 = false;
        $scope.checkboxModel.value2 = false;
        $scope.checkboxModel.value3 = false;
    }

    $scope.cancel = function () {
        $scope.showModal = false;
    };

    $scope.checkboxModel = {
        value1: false,
        value2: false,
        value3: false
    };

    //Muerstra dialog par editar registro
    $scope.alerta = function (uidRow, roleName, objectName, tableName, Permission) {
        $scope.showModal = true;
        $scope.idSelectedRow = uidRow;
        $scope.objectName = objectName;

        //sete los checkBox

        if (Permission.indexOf("R") > -1)
            $scope.checkboxModel.value1 = true
        else
            $scope.checkboxModel.value1 = false

        if (Permission.indexOf("U") > -1)
            $scope.checkboxModel.value2 = true
        else
            $scope.checkboxModel.value2 = false

        if (Permission.indexOf("D") > -1)
            $scope.checkboxModel.value3 = true
        else
            $scope.checkboxModel.value3 = false
        ObjectPermissionServices.GetListRoles(objectName, tableName).then(function (d) {
            $scope.selectableRoles = d.data;
            $scope.selectedOptionRoles = { Rolename: roleName };
        }, function (error) {
            alert('Error!');
        });

    }

    $scope.RoleChange = function () {
        ClearChecks();
        for (var i = 0; i < $scope.data.length; i++) {
            if ($scope.data[i].ObjectName === $scope.objectName && $scope.data[i].Role.Rolename === $scope.selectedOptionRoles.Rolename) {
                if ($scope.data[i].Permission.indexOf("R") > -1)
                    $scope.checkboxModel.value1 = true
                else
                    $scope.checkboxModel.value1 = false

                if ($scope.data[i].Permission.indexOf("U") > -1)
                    $scope.checkboxModel.value2 = true
                else
                    $scope.checkboxModel.value2 = false

                if ($scope.data[i].Permission.indexOf("D") > -1)
                    $scope.checkboxModel.value3 = true
                else
                    $scope.checkboxModel.value3 = false
                return true;
            }
        }

    }

    //Lista de tablas disponibles que controlan permisos por objeto
    $scope.selecttableName = [
       { Value: "Categories", Text: "Categorias" },
       { Value: "Jerarquias", Text: "Jerarquias" }
    ];

    //Selecciona el primer registro de la lista de tablas
    $scope.selectedOption = $scope.selecttableName[0];

    //Lista para cargar los roles de la bbdd
    $scope.selectableRoles = null;
    $scope.selectedOptionRoles = null;


    //Carga el valor nuevo de la lista de roles
    $scope.$watch('selectedOptionRoles', function (newValue, oldValue) {
        if (newValue != null)
            $scope.selectedRole = newValue.Rolename;
    });

    //Carga el resultado de la consulta en la lista cuando se cambia el valor en el <select>
    $scope.GetList = function () {
        ObjectPermissionServices.GetList($scope.selectedOption.Value).then(function (d) {
            $scope.data = d.data;

        }, function (error) {
            alert('Error!');
        });
    }


    //Carga el resultado de la consulta en la lista cuando se carga la pantalla

    ObjectPermissionServices.GetList($scope.selectedOption.Value).then(function (d) {
        $scope.data = d.data;

    }, function (error) {
        alert('Error!');
    });



})

registrationModule.factory('ObjectPermissionServices', function ($http, $q) {
    var fac = {};

    //Se hace llamado http a GetObjectConfiguration para cargar la lista de objetos de la tabla seleccionada
    fac.GetList = function (objectName) {
        return $http.get('/ObjectPermissions/GetObjectConfigurationByTableName?tableName=' + objectName)
    }

    fac.GetListRoles = function (objectName, tableName) {
        return $http.get('/ObjectPermissions/GetListRoles?objectName=' + objectName + '&tableName=' + tableName)
    }

    fac.SaveData = function ($scope) {
        var defer = $q.defer();
        $http({
            url: '/ObjectPermissions/UpdateObjectPermmision',
            method: 'POST',
            data: { 'table': $scope.selectedOption.Value, 'objectName': $scope.objectName, 'permission': $scope.arrpermissions.toString(), 'roleName': $scope.selectedRole },

            headers: { 'content-type': 'application/json' }
        }).success(function (d) {

            defer.resolve(d);
        }).error(function (e) {
            //Failed Callback
            alert(e);
            $scope.showModal = true;
            defer.reject(e);
        });
        return defer.promise;
    }


    return fac;
});

