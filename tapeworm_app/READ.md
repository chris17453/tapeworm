#tapeworm is yaffp. (yet another flat file parser)

## What it does 
- deliver paginated output with variable page lengths
- outputs data in ASCII, json or yaml
- blank, commented or errored data records can be hidden / ignored
- base file can be resaved

## How it does it
- reads from configured target soures
- targets sources are configured in yaml format
- uses object graph deserialization for fast accurate data manipulation

## this is the config format:
- {$model_definitions}/filename.yml
- $model_definition variable is defined in appsettings.json
- fields with a default may be omitted
```
---
name: freds_file                                   # the name to use when searching the file
display: "My File"                                 # the human readable name of this data stream
entity: "bobs shop"                                # the customer/owner/company this data stream belongs to  
group: "Basic"                                     # the grouping to catagorize this data stream  
ordinal: 2                                         # the order of this datastream in the group               , default -1 (none)
uid: 2                                             # the unique identifier for this data stream
path: '/docs/freds.txt'                            # the file path
regex: "^[]$"                                      # a regex to use for validating a single line             , default ''
field_delimiter:                                   # how to split the line                                   , default ,
array_delimiter:                                   # how to split the field                                  , default |
comment_delimiter:                                 # how to split the field                                  , default #
key:                                               # which field is the primary key, not requuired
comments_visible:                                  # do comments count as records                            , default false
data_starts_on_line:1                              # what line to begin reading data                         , default 1
errors_visible: true                               # do entries with errors count as records                 , default true
empty_lines_visible:                               # do empty lines count as records                         , default false
multi_search_enabled: true                         # allow terms to search all enabeled filterable columns   , default true
active: true                                       # is this data stream active                              , default false
properties:                                        # an array of fields per line
- name: node                                       # the property name
  display: 'machine name'                          # the human visible display name of the property           
  type: string                                     # the type of data. int or string                         , default string
  default: ''                                      # default value for this property                         , default for type, string='',int=0
  is_array: 'false'                                # this property is an array of type's                     , default false
  has_default:false                                # use a default value if none is given                    , default false
  ordinal:0                                        # what is the order of this property                      , default -1 (none)
  visible:true                                     # is this property visible                                , default true
  fixed_width:true                                 # can this column auto grow                               , default true only 1 column can have this property. set it to false for that column
  width:100                                        # the default width for this property                     , default 100
  max_width:0                                      # dont mess with, ui stuff                                , default 0
  min_width:0                                      # dont mess with, ui stuff                                , default 0
  overflow:false                                   # dont mess with, ui stuff                                , default false
  filterable:true                                  # is this property searchable                             , default false
  multi_search                                     # is this property available in the multi search          , default false
  sortable                                         # is this property sortable                               , default false
  sort_ordinal                                     # what order to apply this sort                           , default 0
  sort_default                                     # is this a property that is sorted by default            , default false
  sort_default_asc                                 # the default direction of this sort                      , default false
  export:true                                      # can this property be exported                           , default true
  options:[{'key','value'}}]                       # predefined fiolterable options for this property        , 

```

