function UploadTextCtrl($scope, $http) {

    $scope.upload = function () {
        var textResult = new Object();
        textResult.Content = $scope.text;
        textResult.Title = $scope.title;
        textResult.Author = $scope.author;
        textResult.Description = $scope.description;
        textResult.Tags = $scope.tags;
        textResult.Source = $scope.source;
        $http.post(urlRoot + 'api/DataApi/PostText', textResult).success(function (id) {
            clearForm();
            window.location = urlRoot + 'TextView/Index?textID=' + id;

        });
    }

    function clearForm() {
        $scope.text = "";
        $scope.author = "";
        $scope.description = "";
        $scope.searchQuery = "";
        $scope.tagQuery = "";
        $scope.title = "";
        $scope.tags = "";
        $scope.source = "";
    }
}