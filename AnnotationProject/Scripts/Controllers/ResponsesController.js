function ResponsesCtrl($scope, $http) {
    $http.get(urlRoot + 'api/dataapi/getNotifications').success(function (result) {
        $scope.responses = result;
    });
}