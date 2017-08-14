var registrationModule = angular.module('registrationModule', ['ngRoute']) //Se crea el módulo
.config(function ($routeProvider, $locationProvider) {
    $routeProvider.when('/Request/RequestsByParamProcedure', { templateUrl: '/templates/test.html', controller: 'workListController' });
    $locationProvider.html5Mode(false);
});