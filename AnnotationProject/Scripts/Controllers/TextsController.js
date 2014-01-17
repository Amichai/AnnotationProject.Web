function TextsCtrl($scope, $http) {
    $http.get(urlRoot + 'api/DataApi/getAll').success(function (result) {
        $scope.results = result;
    });

    $scope.searchBySelection = "Title";


    $scope.searchBy = function (by) {
        $scope.searchBySelection = by;
    }

    $scope.tableFunction = "Recent Texts";

    $scope.search = function () {
        var title = "";
        var author = "";
        var tags = "";
        if ($scope.searchBySelection == "Author") {
            author = $scope.searchQuery;
        } else if ($scope.searchBySelection == "Tags") {
            tags = $scope.searchQuery;
        } else {
            title = $scope.searchQuery;
        }
        
        $http.get(urlRoot + 'api/DataApi/getText?title=' + title
            + '&tags=' +tags + '&author=' + author).success(function (result) {
            $scope.results = result;
            $scope.tableFunction = "Search Results";
        });
    }

    $scope.clear = function () {
        $scope.searchQuery = "";
        $http.get(urlRoot + 'api/DataApi/getAll').success(function (result) {
            $scope.results = result;
        });
    }
}