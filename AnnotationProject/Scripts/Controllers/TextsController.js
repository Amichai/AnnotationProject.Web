function TextsCtrl($scope, $http) {
    var QueryString = function () {
        // This function is anonymous, is executed immediately and 
        // the return value is assigned to QueryString!
        var query_string = {};
        var query = window.location.search.substring(1);
        var vars = query.split("&");
        for (var i = 0; i < vars.length; i++) {
            var pair = vars[i].split("=");
            // If first entry with this name
            if (typeof query_string[pair[0]] === "undefined") {
                query_string[pair[0]] = pair[1];
                // If second entry with this name
            } else if (typeof query_string[pair[0]] === "string") {
                var arr = [query_string[pair[0]], pair[1]];
                query_string[pair[0]] = arr;
                // If third or later entry with this name
            } else {
                query_string[pair[0]].push(pair[1]);
            }
        }
        if (query_string["author"] != undefined) {
            var authorName = query_string["author"];
            $scope.searchQuery = decodeURI(authorName);
            $scope.searchBySelection = "Author";
        }
        ///TODO: start checking the annotationID parameter
        ///If this is set, query the annotations by that annotation id
        return query_string;
    }();

    $scope.search = function () {
        if ($scope.searchBySelection == "All") {
            var query = $scope.searchQuery;
            $http.get(urlRoot + 'api/DataApi/searchTexts?query=' + query).success(function (result) {
                $scope.results = result;
                $scope.tableFunction = "Search Results";
            });
            return;
        }

        var title = "";
        var author = "";
        var tags = "";
        if ($scope.searchBySelection == "Author") {
            author = $scope.searchQuery;
        } else if ($scope.searchBySelection == "Tags") {
            tags = $scope.searchQuery;
        } else if ($scope.searchBySelection == "Title") {
            title = $scope.searchQuery;
        }

        $http.get(urlRoot + 'api/DataApi/getText?title=' + title + '&tags=' + tags + '&author=' + author).success(function (result) {
                $scope.results = result;
                $scope.tableFunction = "Search Results";
                $scope.busy = false;
            });
    }


    $scope.busy = true;

    if ($scope.searchQuery == undefined || $scope.searchQuery == "") {
        $http.get(urlRoot + 'api/DataApi/getAll').success(function (result) {
            $scope.results = result;
            $scope.busy = false;
        });
    } else {
        $scope.search();
    }

    if ($scope.searchBySelection == undefined || $scope.searchBySelection == "") {
        $scope.searchBySelection = "Title";
    }


    $scope.searchBy = function (by) {
        $scope.searchBySelection = by;
    }

    $scope.tableFunction = "Recent Texts";

    $scope.clear = function () {
        $scope.searchQuery = "";
        $http.get(urlRoot + 'api/DataApi/getAll').success(function (result) {
            $scope.results = result;
            $scope.tableFunction = "Recent Texts";
        });
    }


}