function HomePageCtrl($scope, $http) {
    $scope.text = "";
    $scope.author = "";
    $scope.description = "";
    $scope.searchQuery = "";
    $scope.tagQuery = "";
    $scope.authorQuery = "";
    $scope.title = "";
    $scope.tags = "";

    
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

    function clearForm(){ 
        $scope.text = "";
        $scope.author = "";
        $scope.description = "";
        $scope.searchQuery = "";
        $scope.tagQuery = "";
        $scope.title = "";
        $scope.tags = "";
    }

    $scope.upload = function () {
        var textResult = new Object();
        textResult.Content = $scope.text;
        textResult.Title = $scope.title;
        textResult.Author = $scope.author;
        textResult.Description = $scope.description;
        textResult.Tags = $scope.tags;
        $http.post(urlRoot + 'api/DataApi/PostText', textResult).success(function () {
            clearForm();
        });
    }
}