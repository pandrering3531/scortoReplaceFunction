$(function(){
	//create a new instance of jsNotifications class and set up the general settings 
	var objInstanceName=new jsNotifications({
		autoCloseTime : 5,
		showAlerts: true,
		title: 'My application name'
	});
	
	//check the browser support
	if(objInstanceName.isAvailable()){
		//show the bar to Chrome/Chromium users
		if(objInstanceName.getStatus()==1) $('#divBottomBar').fadeIn(1200);
	}
	
	//info. message
	$('#btnInfo').on('click',function(){
		objInstanceName.show('info','Sticky note. This notification won\'t disappear. '+
		'You have to close it ;)',true);
	});
	
	//error message
	$('#btnError').on('click',function(){
		objInstanceName.show('error','Hello world',false,
		'Error notification with customized title');
	});
	
	//warning message
	$('#btnWarning').on('click',function(){
		objInstanceName.show('warning','Hello world');
	});
	
	//ok message
	$('#btnOK').on('click',function(){
		objInstanceName.show('ok','Hello world');
	});
	
	//show html notification
	$('#btnHTML').on('click',function(){
		objInstanceName.showHTML('message.html',true);
	});
	
	//click on the bottom bar
	$('#divBottomBar').on('click',function(){
		objInstanceName.requestPermission(function(){
			$('#divBottomBar').fadeOut();
		});
	});
	
});