var searchVisible = 0;
var transparent = true;

var transparentDemo = true;
var fixedTop = false;

var navbar_initialized = false;


//firebase & database handling variables
var database = firebase.database();
var RefUsers = database.ref("/Users/Arthur/Prescription/");
var RefUsers2 = database.ref("/Users/Arthur/");

var pres = [];
var curr_counter;
var curr_user;

//toggle user
function toggleUser(name) {
    curr_user = name;
    var string1 = "/Users/"+curr_user+"/Prescription/";
    var string2 = "/Users/"+curr_user+"/";
    window.alert(string1);
    var RefUsers = database.ref(string1);
    var RefUsers2 = database.ref(string2);
    pres = [];
    /*RefUsers.once('value').then(function(snapshot) {
        snapshot.forEach(function(childSnapshot) {
            //window.alert(childSnapshot.getKey());
            var childDescription = childSnapshot.val().Description;
            var childDosage = childSnapshot.val().Dosage;
            var childName = childSnapshot.val().Name;
            var childTaken = childSnapshot.val().Taken;
            var childTime = childSnapshot.val().Time;
            var childType = childSnapshot.val().Type;
            var temp = new Prescription(childDescription,childDosage, childName, childTaken, childTime, childType);
            pres.push(temp);
        });
        updateTable();
    });*/

}

//custom Prescription object
function Prescription(description, dosage, name, state, time, type) {
    this.description = description;
    this.dosage = dosage;
    this.name = name;
    this.state = state;
    this.time = time;
    this.type = type;
    //return this;

};

function updateTable() {
    var color="";
    var table = "<table class='table'> <tbody>";
    for (j = 0; j<pres.length; j++) {
        console.log("Taken: " + pres[j].state);
        if (pres[j].state == 1) {color="bg-success";}
        else {color = "bg-danger";}
        table += "<tr class='"+color+"'> <td>";
        table += pres[j].name.toString();
        table += "</td>";
        table += "<td class='td-actions text-right'>";
        table+="<button type='button' rel='tooltip' title='Edit Task'; class='btn btn-info btn-simple btn-xs'><i class='fa fa-edit'></i> </button>";
        table+="<button type='button; rel='tooltip' title='Remove' class='btn btn-danger btn-simple btn-xs'><i class='fa fa-times'></i> </button>";
        table+="</td>";
        table += "</tr>"; 
    }
    table += "</tbody> </table>";

    console.log(table);
    $("#replace_table").empty();
    $("#replace_table").html(table);

};

function updatePrescriptions() {
    //window.alert("entering function");
    curr_counter = 0;
    console.log(curr_counter);
    RefUsers.once('value').then(function(snapshot) {
        snapshot.forEach(function(childSnapshot) {
            //window.alert(childSnapshot.getKey());
            var childDescription = childSnapshot.val().Description;
            var childDosage = childSnapshot.val().Dosage;
            var childName = childSnapshot.val().Name;
            var childTaken = childSnapshot.val().Taken;
            var childTime = childSnapshot.val().Time;
            var childType = childSnapshot.val().Type;
            var temp = new Prescription(childDescription,childDosage, childName, childTaken, childTime, childType);
            pres.push(temp);
        });
        updateTable();
    });
};

RefUsers2.on('child_changed',function(snapshot) {
        pres = [];
        snapshot.forEach(function(childSnapshot) {
            //window.alert(childSnapshot.getKey());
            var childDescription = childSnapshot.val().Description;
            var childDosage = childSnapshot.val().Dosage;
            var childName = childSnapshot.val().Name;
            var childTaken = childSnapshot.val().Taken;
            var childTime = childSnapshot.val().Time;
            var childType = childSnapshot.val().Type;
            var temp = new Prescription(childDescription,childDosage, childName, childTaken, childTime, childType);
           
            pres.push(temp);

        });
        //document.location.reload(true);
        
        updateTable();
});



$(document).ready(function(){
    window_width = $(window).width();
    
    // check if there is an image set for the sidebar's background
    lbd.checkSidebarImage();
    
    // Init navigation toggle for small screens   
    if(window_width <= 991){
        lbd.initRightMenu();   
    }
     
    //  Activate the tooltips   
    $('[rel="tooltip"]').tooltip();

    //      Activate the switches with icons 
    if($('.switch').length != 0){
        $('.switch')['bootstrapSwitch']();
    }  
    //      Activate regular switches
    if($("[data-toggle='switch']").length != 0){
         $("[data-toggle='switch']").wrap('<div class="switch" />').parent().bootstrapSwitch();     
    }
     
    $('.form-control').on("focus", function(){
        $(this).parent('.input-group').addClass("input-group-focus");
    }).on("blur", function(){
        $(this).parent(".input-group").removeClass("input-group-focus");
    });
    //handle firebase data handling
    updatePrescriptions();
   
    
      
});


// activate collapse right menu when the windows is resized 
$(window).resize(function(){
    if($(window).width() <= 991){
        lbd.initRightMenu();   
    }
});
    
lbd = {
    misc:{
        navbar_menu_visible: 0
    },
    
    checkSidebarImage: function(){
        $sidebar = $('.sidebar');
        image_src = $sidebar.data('image');
        
        if(image_src !== undefined){
            sidebar_container = '<div class="sidebar-background" style="background-image: url(' + image_src + ') "/>'
            $sidebar.append(sidebar_container);
        }  
    },
    initRightMenu: function(){  
         if(!navbar_initialized){
            $navbar = $('nav').find('.navbar-collapse').first().clone(true);
            
            $sidebar = $('.sidebar');
            sidebar_color = $sidebar.data('color');
            
            $logo = $sidebar.find('.logo').first();
            logo_content = $logo[0].outerHTML;
                    
            ul_content = '';
             
            $navbar.attr('data-color',sidebar_color);
             
            // add the content from the sidebar to the right menu
            content_buff = $sidebar.find('.nav').html();
            ul_content = ul_content + content_buff;
            
            //add the content from the regular header to the right menu
            $navbar.children('ul').each(function(){
                content_buff = $(this).html();
                ul_content = ul_content + content_buff;   
            });
             
            ul_content = '<ul class="nav navbar-nav">' + ul_content + '</ul>';
            
            navbar_content = logo_content + ul_content;
            
            $navbar.html(navbar_content);
             
            $('body').append($navbar);
             
            background_image = $sidebar.data('image');
            if(background_image != undefined){
                $navbar.css('background',"url('" + background_image + "')")
                       .removeAttr('data-nav-image')
                       .addClass('has-image');                
            }
             
             
             $toggle = $('.navbar-toggle');
             
             $navbar.find('a').removeClass('btn btn-round btn-default');
             $navbar.find('button').removeClass('btn-round btn-fill btn-info btn-primary btn-success btn-danger btn-warning btn-neutral');
             $navbar.find('button').addClass('btn-simple btn-block');
            
             $toggle.click(function (){    
                if(lbd.misc.navbar_menu_visible == 1) {
                    $('html').removeClass('nav-open'); 
                    lbd.misc.navbar_menu_visible = 0;
                    $('#bodyClick').remove();
                     setTimeout(function(){
                        $toggle.removeClass('toggled');
                     }, 400);
                
                } else {
                    setTimeout(function(){
                        $toggle.addClass('toggled');
                    }, 430);
                    
                    div = '<div id="bodyClick"></div>';
                    $(div).appendTo("body").click(function() {
                        $('html').removeClass('nav-open');
                        lbd.misc.navbar_menu_visible = 0;
                        $('#bodyClick').remove();
                         setTimeout(function(){
                            $toggle.removeClass('toggled');
                         }, 400);
                    });
                   
                    $('html').addClass('nav-open');
                    lbd.misc.navbar_menu_visible = 1;
                    
                }
            });
            navbar_initialized = true;
        }
   
    }
}


// Returns a function, that, as long as it continues to be invoked, will not
// be triggered. The function will be called after it stops being called for
// N milliseconds. If `immediate` is passed, trigger the function on the
// leading edge, instead of the trailing.

function debounce(func, wait, immediate) {
	var timeout;
	return function() {
		var context = this, args = arguments;
		clearTimeout(timeout);
		timeout = setTimeout(function() {
			timeout = null;
			if (!immediate) func.apply(context, args);
		}, wait);
		if (immediate && !timeout) func.apply(context, args);
	};
};
