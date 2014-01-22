function UsersCtrl($scope, $http) {
    $scope.busy = true;
    $http.get(urlRoot + 'api/DataApi/getSiteUsers').success(function (result) {
        $scope.users = result;
        $scope.busy = false;
    });
}