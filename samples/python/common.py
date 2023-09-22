import collections

class CustomType(collections.UserDict):
    def __init__(self, name, id, field):
        super().__init__()
        self['Name'] = name
        self['Id'] = id
        self['Field'] = field
