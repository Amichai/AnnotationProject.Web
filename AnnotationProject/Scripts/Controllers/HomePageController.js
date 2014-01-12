function HomePageCtrl($scope, $http) {
    $scope.searchQuery = "";
    $scope.tagQuery = "";
    $scope.authorQuery = "";

    
    $scope.results = new Object();

    $scope.search = function () {
        $http.get(urlRoot + 'api/DataApi/getText?title=' + $scope.searchQuery + '&tags=' + $scope.tagQuery + '&author=' + $scope.authorQuery).success(function (result) {
            $scope.results = result;
        });
    }

    $http.get(urlRoot + 'api/DataApi/getAll').success(function (result) {
        $scope.results = result;
        });
        
    $http.get(urlRoot + 'api/DataApi/recentAnnotations').success(function (result) {
        $scope.recentAnnotations = result;
    });
}