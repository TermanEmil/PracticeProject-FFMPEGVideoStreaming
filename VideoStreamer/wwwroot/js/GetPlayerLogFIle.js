function updateFunction(channel, hlsListSize, interval)
{
    $.ajax(
    {
        type: "POST",
        url: "/api/LogFile/GetData?channel=" + channel,
        dataType: 'json',
        contentType: 'application/json',

         success: function(result)
         {
                console.log(result);
                document.getElementById("log-file").value = result;
         },
         error: function(xhr, textStatus, error)
         {

         }
     });
}

