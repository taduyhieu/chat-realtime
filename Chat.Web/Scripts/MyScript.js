$(function () {

    $('ul#user-list').on('click', 'li', function () {
        var username = $("input[type=hidden].username", $(this)).val();
        var input = $('#chat-message');

        var text = input.val();
        if (text.startsWith("/")) {
            text = text.split(")")[1];
        }

        //text = "/private(" + username + ") " + text;
        //input.val(text);
        input.focus();
    });
    
    $('#emojis-container').on('click', 'a', function () {
        var value = $("input", $(this)).val();
        var input = $('#chat-message');

        input.val(input.val() + value);
        input.focus();
    });

    // Show/Hide Emoji Window
    $("#emojibtn").click(function () {
        var x = $("#emojis-container");
        if (x.hasClass("hidden")) {
            x.removeClass("hidden");
        }
        else {
            x.addClass("hidden");
        }
    });

    $("#chat-message").click(function () {
        $("#emojis-container").addClass("hidden")
    });

    // Upload image with Ajax
    $("#btnUpload").change(function () {
        console.log("hieu");
        var data = new FormData();
        let userReceiverId = $("#userReceiverId").val();
        var file = document.getElementById("btnUpload").files[0];

        data.append("btnUpload", file);
        data.append("userReceiverId", userReceiverId);
        console.log(data);
        $.ajax({
            type: "POST",
            url: '/Home/Upload',
            data: data,
            dataType: 'json',
            contentType: false,
            processData: false,
            success: function (response) {
                console.log(response);
            },
            error: function (error) {
                console.log(error);
            }
        });

        

    });
});