thisdir = class/System.Activities
SUBDIRS = 
include ../../build/rules.make

RESOURCE_FILES =

LIBRARY = System.Activities.dll
LIB_MCS_FLAGS = \
			/nowarn:168,169,219,414 \
	        /r:System.dll \
	        /r:System.Xml.dll \
		/r:System.Core.dll \
	        /r:System.Xml.Linq.dll \
		/r:System.Xaml.dll \
	        /r:System.Transactions.dll \
		/r:System.Runtime.Serialization.dll \
		/r:System.Runtime.DurableInstancing.dll \
	        $(RESOURCE_FILES:%=/resource:%)

ifneq (2.1, $(FRAMEWORK_VERSION))
LIB_MCS_FLAGS += /r:System.Data.dll /r:System.Configuration.dll
endif

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) /r:System.ServiceModel.dll

EXTRA_DISTFILES = $(RESOURCE_FILES)

VALID_PROFILE := $(filter 4.0, $(FRAMEWORK_VERSION))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-System.Activities.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
endif

include ../../build/library.make
