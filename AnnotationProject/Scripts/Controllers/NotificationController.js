function NotificationCtrl($scope, $http) {
    $http.get(urlRoot + 'api/dataapi/getNotifications').success(function (result) {
        $scope.notifications = result;
    });
}