#
# Copyright (c) 2012 Code Owls LLC
#
# Permission is hereby granted, free of charge, to any person obtaining a copy 
# of this software and associated documentation files (the "Software"), to 
# deal in the Software without restriction, including without limitation the 
# rights to use, copy, modify, merge, publish, distribute, sublicense, and/or 
# sell copies of the Software, and to permit persons to whom the Software is 
# furnished to do so, subject to the following conditions:
#
# The above copyright notice and this permission notice shall be included in 
# all copies or substantial portions of the Software.
#
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
# IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
# AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
# LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
# FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS 
# IN THE SOFTWARE. 

function mount-sqlite
{
	param(
		[Parameter(Mandatory=$true)]
		[string]
		[Alias( 'DriveName' )]
		$name,
		
		[Parameter()]
		[string]
		$dataSource = ':memory:'		
	);
	
	process
	{
		New-PSDrive -Name $name -PSProvider SQLite -Root "Data Source=$dataSource" -Scope Global;
	}
	
<#
.SYNOPSIS 
Mounts a new SQLite drive in your PowerShell session.

.DESCRIPTION
Mounts a new SQLite drive in your PowerShell session.

By default this cmdlet will mount a transient memory-only database.  You can specify a datafile to use a persistent database.

.INPUTS
This command accepts no input.

.OUTPUTS
The new PowerShell drive.

.EXAMPLE
C:\> mount-sqlite -name memdb

Creates a new in-memory database and mounts it to the drive named memdb.

.EXAMPLE
C:\> mount-sqlite -name db -dataSource data.sqlite

Creates a persistent database and mounts it to the drive named db.  If the data.sqlite data file already exists, the existing database is mounted.

.NOTES
	#requires -version 3
#>
}

Export-ModuleMember -Function *sqlite*