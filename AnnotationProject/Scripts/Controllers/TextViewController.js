﻿/// <reference path="../linq.js_ver2.2.0.2/linq.js" />
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

    $scope.editModeTextBoxClick = function (evt) {
        evt.stopPropagation();
    }

    $scope.saveAnnotationEdit = function (p) {
        var ann = $scope.annotations[p.idx];
        $http.post(urlRoot + 'api/DataApi/updateAnnotation', ann).success(function (result) {
            $scope.allAnnotations = result;
            $scope.annotations = result;
        });
    }

    $scope.favorite = function (p) {
        var ann = $scope.annotations[p.idx];
        $http.post(urlRoot + 'api/DataApi/FavoriteAnnotation?annotationID=' + ann.AnnotationID ).success(function () {
            ann.UserFavorited = !ann.UserFavorited;
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
        $http.get(urlRoot + 'api/DataApi/getAnnotations?textID=' + $scope.textID).success(function (result) {
            $scope.allAnnotations = result;
            $scope.annotations = result;
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

    $scope.annotationSearchUpdate = function () {
        ///TODO: split on spaces, commas and treat each token independently
        $scope.annotations = Enumerable.From($scope.allAnnotations).Where(function (x) {
            var result = x.Content.toLowerCase().indexOf($scope.annotationSearch.toLowerCase()) != -1 ||
                 x.TextAnchor.toLowerCase().indexOf($scope.annotationSearch.toLowerCase()) != -1 ||
                 x.Tags.indexOf($scope.annotationSearch) != -1;
            return result;
        }).ToArray();
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
    
}