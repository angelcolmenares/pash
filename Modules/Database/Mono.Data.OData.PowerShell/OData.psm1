function mount-OData
{
	param( 
		[parameter( Mandatory=$true, valueFromPipelineByPropertyName=$true )]
		[string]
		[alias( 'drivename' )]
		$name,

		[parameter( Mandatory=$true, valueFromPipelineByPropertyName=$true )]
		[string]
		$uri
	)

	new-psdrive -psprovider odata -root $uri -name $name;

}