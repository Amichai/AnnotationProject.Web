function ArchivedTextsCtrl($scope, $http) {
    $http.get(urlRoot + 'api/DataApi/MembershipTest').success(function (result) {
        console.log(result);
    });

    $http.get(urlRoot + 'api/DataApi/GetArchivedTexts').success(function (result) {
        $scope.texts = result;
    });
    

    $scope.restoreText = function (idx) {
        var id = $scope.texts[idx].ID;
        $http.post(urlRoot + 'api/dataApi/RestoreText?id=' + id).success(function (result) {
            $scope.texts = result;
        });
    };
}