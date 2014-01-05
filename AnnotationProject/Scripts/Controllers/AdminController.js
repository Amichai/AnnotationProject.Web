function AdminCtrl($scope, $http) {
    $http.get(urlRoot + 'api/DataApi/MembershipTest').success(function (result) {
        console.log(result);
    });

	$http.get(urlRoot + 'api/DataApi/getUsers').success(function(result){
	    $scope.users = result;
	    console.log(result.length + ' users');
	});
	$scope.addRole = function (idx) {
	    var user = $scope.users[idx].Username;
	    var role = $scope.users[idx].RoleToAdd;
	    $http.post(urlRoot + 'api/dataApi/addRole?user=' + user + '&role=' + role).success(function (result) {
	        $scope.users = result;
	    });
	};

	$scope.removeRole = function (idx) {
	    var user = $scope.users[idx].Username;
	    var role = $scope.users[idx].RoleToAdd;
	    $http.post(urlRoot + 'api/dataApi/removeRole?user=' + user + '&role=' + role).success(function (result) {
	        $scope.users = result;
	    });
	};
}