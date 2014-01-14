/// <reference path="../linq.js_ver2.2.0.2/linq.js" />
function TextViewCtrl($scope, $http) {
    $scope.annotation = "";
    $scope.anchor = "";
    $scope.matches = 0;
    $scope.text = "";
    $scope.title = "";
    $scope.author = "";
    $scope.description = "";
    $scope.tags = "";
    $scope.source = "";
    $scope.annotationTags = "";
    $scope.annotationSource = "";
    $scope.uploader = "";
    $scope.nextText = "";
    $scope.prevText = "";
    $scope.isBaseText = true;

    $scope.annotationSearch = "";

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

    $scope.expandAnnotationInText = function (idx) {
        if ($scope.annotationsAndText[idx].TextAnchor == undefined) {
            return;
        }
        if (idx != expandedIdx && expandedIdx != -1) {
            $scope.annotationsAndText[expandedIdx].Expanded = false;
        }
        $scope.annotationsAndText[idx].Expanded = !$scope.annotationsAndText[idx].Expanded;
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
        newAnnotation.Tags = $scope.annotationTags;
        newAnnotation.Source = $scope.annotationSource;
        $http.post(urlRoot + 'api/DataApi/PostAnnotation', newAnnotation).success(function (annotations) {
            $scope.annotation = "";
            $scope.anchor = "";
            $scope.annotationTags = "";
            $scope.annotationSource = "";
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
        $scope.annotationIDQuery = query_string["annotationID"];
        
        if ($scope.annotationIDQuery != undefined) {
            $scope.annotationSearch = "id:" + $scope.annotationIDQuery;
        }
        ///TODO: start checking the annotationID parameter
        ///If this is set, query the annotations by that annotation id
        return query_string;
    }();

    function filterById(id) {
        return Enumerable.From($scope.allAnnotations).Where(function (x) {
            var result = x.AnnotationID == id;
            return result;
        }).ToArray();
    }

    var highlightedText = "";

    function highlight(text) {
        if (text == highlightedText) {
            return;
        }
        var innerHTML = $scope.text;
        var index = 0;
        var prefix = "<span class='highlight'>";
        var loopCounter = 0;
        while (index != -1) {
            index = innerHTML.indexOf(text, index);
            if (index == -1) {
                break;
            }
            innerHTML = innerHTML.substring(0, index) + prefix + text + "</span>" + innerHTML.substring(index + text.length);
            if (index != -1) {
                index += prefix.length + 7;
            } else {
                break;
            }
            if (loopCounter++ > 50) {
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
        $http.post(urlRoot + 'api/DataApi/ArchiveAnnotation?annotationID=' + annotID + '&textID=' + baseTextID).success(function (result) {
            expandedIdx = -1;
            $scope.allAnnotations = result;
            $scope.annotations = result;
        });
        p.ev.stopPropagation();
    }

    $scope.seeComments = function (p) {
        //debugger;
        var id = $scope.annotations[p.idx].TextID;
        p.ev.stopPropagation();
        window.location = urlRoot + 'textView/Index?textID=' + id;
    }

    $scope.editModeTextBoxClick = function (evt) {
        evt.stopPropagation();
    }

    $scope.stopPropagation = function (evt) {
        evt.stopPropagation();
    }

    $scope.saveAnnotationEdit = function (p) {
        var ann = $scope.annotations[p.idx];
        $http.post(urlRoot + 'api/DataApi/updateAnnotation', ann).success(function (result) {
            $scope.allAnnotations = result;
            $scope.annotationSearchUpdate();
        });
    }

    $scope.favorite = function (p) {
        var ann = $scope.annotations[p.idx];
        $http.post(urlRoot + 'api/DataApi/FavoriteAnnotation?annotationID=' + ann.AnnotationID ).success(function () {
            ann.UserFavorited = !ann.UserFavorited;
            if (ann.UserFavorited) {
                ann.FavoriteCount++;
            } else {
                ann.FavoriteCount--;
            }
        });
        p.ev.stopPropagation();
    }

    $http.get(urlRoot + 'api/DataApi/getText?id=' + $scope.textID).success(function (result) {
        $scope.text = result.Content;
        $scope.title = result.Title;
        $scope.author = result.Author;
        $scope.description = result.Description;
        $scope.uploader = result.Uploader;
        $scope.source = result.Source;
        $scope.tags = result.Tags;
        $scope.nextText = result.NextText;
        $scope.prevText = result.PrevText;
        $scope.isBaseText = result.IsBaseText;
        $http.get(urlRoot + 'api/DataApi/getAnnotations?textID=' + $scope.textID).success(function (result) {
            $scope.allAnnotations = result;
            $scope.annotations = result;
            if (QueryString.annotationID != undefined) {
                $scope.annotations = filterById(QueryString.annotationID);
            }
            //$scope.linearLayout = true;
            //setLayoutLinear();
        });
    });

    $scope.updateTextDetails = function () {
        var updatedText = new Object();
        updatedText.Content = $scope.text;
        updatedText.Title = $scope.title;
        updatedText.Author = $scope.author;
        updatedText.Tags = $scope.tags;
        updatedText.Source = $scope.source;
        updatedText.ID = $scope.textID;
        updatedText.PrevText = $scope.prevText;
        updatedText.NextText = $scope.nextText;
        $http.post(urlRoot + 'api/DataApi/updateTextDetails', updatedText).success(function (result) {

        });
    }

    function filterByAnchor(anchor) {
        return Enumerable.From($scope.allAnnotations).Where(function (x) {
            return x.TextAnchor.toLowerCase().indexOf(anchor.toLowerCase()) != -1
        }).ToArray();
    }

    function filterByUser(user) {
        return Enumerable.From($scope.allAnnotations).Where(function (x) {
            var result = x.Username == user;
            return result;
        }).ToArray();
    }

    function filterByFavoritedBy(term) {
        debugger;
    }

    function intersect(a, b) {
        var t;
        if (b.length > a.length) t = b, b = a, a = t; // indexOf to loop over shorter
        return a.filter(function (e) {
            if (b.indexOf(e) !== -1) return true;
        });
    }

    $scope.annotationSearchUpdate = function () {
        if ($scope.annotationSearch == "") {
            $scope.annotations = $scope.allAnnotations;
            return;
        }

        var split = $scope.annotationSearch.split(' ');
        var toShow = $scope.allAnnotations;
        for (var i = 0; i < split.length; i++) {
            var term = split[i];
            if (term == "") {
                continue;
            }
            var qualifier = split[i].split(':');
            if (qualifier.length == 1) {
                var a = Enumerable.From($scope.allAnnotations).Where(function (x) {
                    var result = x.Content.toLowerCase().indexOf(term.toLowerCase()) != -1 ||
                         x.TextAnchor.toLowerCase().indexOf(term.toLowerCase()) != -1 ||
                         x.Tags.indexOf(term.toLowerCase()) != -1;
                    return result;
                }).ToArray();
                toShow = intersect(a, toShow);
            } else {
                if (qualifier[0] == "id") {
                    a = filterById(qualifier[1]);
                    toShow = intersect(a, toShow);
                } else if (qualifier[0] == "user") {
                    a = filterByUser(qualifier[1]);
                    toShow = intersect(a, toShow);
                } else if (qualifier[0] == "anchor") {
                    a = filterByAnchor(qualifier[1]);
                    toShow = intersect(a, toShow);
                } else if (qualifier[0] == "favoritedby") {
                    a = filterByFavoritedBy(qualifier[1]);
                    toShow = intersect(a, toShow);
                }
            }
        }
        $scope.annotations = toShow;


        if ($scope.linearLayout) {
            setLayoutLinear();
        }
        //debugger;
        ///TODO: split on spaces, commas and treat each token independently

    }

    $scope.clearAnnotationSearch = function () {
        $scope.annotationSearch = "";
        $scope.annotations = $scope.allAnnotations;
    }

    $scope.navigatePrevious = function () {
        window.location = urlRoot + 'textView/Index?textID=' + $scope.prevText;
    }

    $scope.navigateNext = function () {
        window.location = urlRoot + 'textView/Index?textID=' + $scope.nextText;
    }

    $scope.sortingScheme = "";

    $scope.sortByFavorited = function () {
        $scope.annotations = Enumerable.From($scope.annotations).OrderByDescending(function (x) {
            return x.FavoriteCount;
        }).ToArray();
        $scope.sortingScheme = "favorited";
    }

    $scope.sortByRecent = function () {
        $scope.annotations = Enumerable.From($scope.annotations).OrderByDescending(function (x) {
            return x.Timestamp;
        }).ToArray();
        $scope.sortingScheme = "recent";
    }

    $scope.setLayoutSideBySide = function(){
        $scope.linearLayout = false;
    }

    function addTextAnnotation(startIdx, endIdx){
        var clip = $scope.text.substring(startIdx, endIdx);
        if (clip != undefined && clip.length > 0) {
            var newAnnotation = new Object();
            newAnnotation.Content = clip;
            $scope.annotationsAndText.push(newAnnotation);
        }
    }

    function setLayoutLinear (){
        $scope.linearLayout = true;

        for (var i = 0; i < $scope.annotations.length; i++) {
            var a = $scope.annotations[i];
            var index = $scope.text.indexOf(a.TextAnchor);
            a.index = index;
        }
        var sorted = Enumerable.From($scope.annotations).OrderBy(function (x) {
            return x.index;
        }).ToArray();
        var startVal = 0;
        $scope.annotationsAndText = new Array();
        for (var i = 0; i < sorted.length; i++) {
            var ann = sorted[i];
            var endVal = ann.index + ann.TextAnchor.length;
            addTextAnnotation(startVal, endVal);
            $scope.annotationsAndText.push(ann);
            startVal = endVal;
            console.log("Break");
        }
        addTextAnnotation(endVal, $scope.text.length);
    }

    $scope.setLayoutLinear = function () {
        setLayoutLinear();
    }
}