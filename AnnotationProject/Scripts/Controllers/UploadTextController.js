function UploadTextCtrl($scope, $http) {
    $scope.text = "";
    $scope.author = "";
    $scope.description = "";
    $scope.searchQuery = "";
    $scope.title = "";

    $scope.results = new Object();

    $scope.search = function () {
        $http.get(urlRoot + 'api/DataApi/getText?query=' + $scope.searchQuery).success(function (result) {
            $scope.results = result;
        });
    }

    function clearForm(){ 
        $scope.text = "";
        $scope.author = "";
        $scope.description = "";
        $scope.searchQuery = "";
        $scope.title = "";
    }

    $scope.upload = function () {
        var textResult = new Object();
        textResult.Content = $scope.text;
        textResult.Title = $scope.title;
        textResult.Author = $scope.author;
        textResult.Description = $scope.description;
        $http.post(urlRoot + 'api/DataApi/PostText', textResult).success(function () {
            clearForm();
        });
    }
}