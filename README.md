# nVader
Philly Codefest Hackathon Project

### API
#### Resource
A resource is an item which may be collected by a player at a given location. This is a parent object and should not be used directly.

##### Methods 

| Method | Param | Return | Description |
| :--- | :--- | :--- | :--- |
| `Resource` | `string` Name, `int` Value | `Resource` | Constructor |
| `GetResourceType` | - | `string` | Returns the type of the resource |
| `GetResourceValue` | - | `int` | Returns how much resource is here in units |
| `AddResourcesOfThisType` | `int` Increment | `void` | Increments a certain resource by a number of units |

##### Children
`TimeEnergy`

#### Landmark
A landmark is a location that is notable
##### Methods

| Method | Param | Return | Description |
| :--- | :--- | :--- | :--- |
| `Landmark` | `string` Title, `string` Description, `List<Resource>` Resources | `Landmark` | Constructor |
| `GetTitle` | | `string` | Returns title of landmark |
| `GetDescription` | | `string` | Returns description of landmark |
| `GetResources` | | `List<Resource>` | Returns a list of resources that are available at this landmark |


