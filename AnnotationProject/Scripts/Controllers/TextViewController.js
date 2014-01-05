function TextViewCtrl($scope, $http) {
    $scope.annotation = "";
    $scope.anchor = "";
    $scope.matches = 0;
    $scope.text = "";
    $scope.title = "";
    $scope.author = "";
    $scope.description = "";

    $scope.isAddAnnotationVisible = false;

    $scope.urlRoot = urlRoot;
    $scope.hover = function (idx) {
        highlight($scope.annotations[idx].TextAnchor);
        //$('.contentBody').scrollTop(100)
    }

    $scope.toggleAddAnnotation = function () {
        $scope.isAddAnnotationVisible = !$scope.isAddAnnotationVisible;
    }

    var expandedIdx = -1;

    $scope.expand = function (idx) {
        if (idx != expandedIdx && expandedIdx != -1) {
            $scope.annotations[expandedIdx].Expanded = false;
        }
        $scope.annotations[idx].Expanded = !$scope.annotations[idx].Expanded;
        expandedIdx = idx;
    }

    $scope.anchorUpdate = function () {
        var re = new RegExp($scope.anchor, "g");
        if ($scope.anchor == "") {
            $scope.matches = 0;
            $('#content').html($scope.text);
            highlightedText = "";
            return;
        }
        var result = $scope.text.match(re);
        if (result == undefined) {
            $scope.matches = 0;
            $('#content').html($scope.text);
            highlightedText = "";
        } else {
            $scope.matches = result.length;
            highlight($scope.anchor);
        }
    }

    $scope.addAnnotation = function (name) {
        var newAnnotation = new Object();
        newAnnotation.Content = $scope.annotation;
        newAnnotation.TextAnchor = $scope.anchor;
        newAnnotation.BaseTextID = $scope.textID;
        newAnnotation.Username = name;
        debugger;
        $http.post(urlRoot + 'api/DataApi/PostAnnotation', newAnnotation).success(function (annotations) {
            $scope.annotation = "";
            $scope.anchor = "";
            $scope.annotations = annotations;
        });
    }

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
        $scope.textID = query_string["textID"];
        return query_string;
    }();

    var highlightedText = "";

    function highlight(text) {
        if (text == highlightedText) {
            return;
        }
        var innerHTML = $scope.text;
        var index = 0;
        var prefix = "<span class='highlight'>";
        while (index != -1) {
            index = innerHTML.indexOf(text, index);
            if (index == -1) {
                break;
            }
            innerHTML = innerHTML.substring(0, index) + prefix + text + "</span>" + innerHTML.substring(index + text.length);
            if (index != -1) {
                index += prefix.length + 1;
            } else {
                break;
            }
        }
        $('#content').html(innerHTML);
        highlightedText = text;
    }

    $scope.editAnnotation = function (p) {
        $scope.annotations[p.idx].EditMode = !$scope.annotations[p.idx].EditMode;
        p.ev.stopPropagation();
    }

    $scope.deleteAnnotation = function (p) {
        var annotID = $scope.annotations[p.idx].TextID;
        var baseTextID = $scope.annotations[p.idx].BaseTextID;
        var username = $scope.annotations[p.idx].Username;
        debugger;
        $http.post(urlRoot + 'api/DataApi/ArchiveAnnotation?annotationID=' + annotID + '&textID=' + baseTextID).success(function (result) {
            $scope.annotations = result;
        });
        p.ev.stopPropagation();
    }

    $scope.editModeTextBoxClick = function (evt) {
        evt.stopPropagation();
    }

    $scope.saveAnnotationEdit = function (p) {
        var ann = $scope.annotations[p.idx];
        $http.post(urlRoot + 'api/DataApi/updateAnnotation', ann).success(function (result) {
            $scope.annotations = result;
        });
    }

    $http.get(urlRoot + 'api/DataApi/getText?id=' + $scope.textID).success(function (result) {
        $scope.text = result.Content;       
        $scope.title = result.Title;
        $scope.author = result.Author;
        $scope.description = result.Description;
        $http.get(urlRoot + 'api/DataApi/getAnnotations?textID=' + $scope.textID).success(function (result) {
            $scope.annotations = result;
        });
    });
}