function ResponsesCtrl($scope, $http) {
    $scope.busy = true;
    $http.get(urlRoot + 'api/dataapi/getNotifications').success(function (result) {
        $scope.responses = result;
        $scope.busy = false;
    });
}