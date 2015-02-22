# nVader
Philly Codefest Hackathon Project

## Client API
#### Utils/Resource
A resource is an item which may be collected by a player at a given location. This is a parent object and should not be used directly.

##### Methods 

| Method | Param | Return | Description |
| :--- | :--- | :--- | :--- |
| `Resource` | `string` Name, `int` Value | `Resource` | Constructor |
| `GetResourceType` | - | `string` | Returns the type of the resource |
| `GetResourceValue` | - | `int` | Returns how much resource is here in units |
| `AddResourcesOfThisType` | `int` Increment | `void` | Increments a certain resource by a number of units |

#### Utils/Time Energy : Resource
Time Energy is a type of resource which refers to the fuel of the spaceship

| Method | Param | Return | Description |
| :--- | :--- | :--- | :--- |
| `TimeEnergy` | `int` Value | `TimeEnergy : Resource` | Constructor |

#### Utils/Landmark
A landmark is a location that is notable
##### Methods

| Method | Param | Return | Description |
| :--- | :--- | :--- | :--- |
| `Landmark` | `string` Title, `string` Description, `List<Resource>` Resources, `int` BuildYear | `Landmark` | Constructor |
| `GetTitle` | | `string` | Returns title of landmark |
| `GetDescription` | | `string` | Returns description of landmark |
| `GetResources` | | `List<Resource>` | Returns a list of resources that are available at this landmark |
| `GetBuildYear` | | `int` | Year the landmark was built |

#### Utils/Beacon
A beacon is a device which identifies any notable local areas
##### Methods

| Method | Param | Return | Description |
| :--- | :--- | :--- | :--- |
| `Beacon` | `Landmark` location | `Beacon` | Constructor with a local landmark passed in

#### Utils/Mine
A mine is a device which is able to harvest various types of resources from a given area

| Method | Param | Return | Description |
| :--- | :--- | :--- | :--- |
| `Mine` | `int` points, `int` harvest, `double[]` coordinates, `Landmark` location | `Mine` | Constructor containing number of hours to harvest, coordinates of location, and the given landmark |
| `GetCoordinatesPlaced` | | `double[]` | Returns a double array giving the current position of the mine |
| `GetHarvestTime` | | `long` | The time in 'ticks' when the mine will finish harvesting the resources | 
| `GetTimePlaced` | | `long` | The time in 'ticks' when the mine was first placed |
| `GetPairedLandmark` | | `Landmark` | Returns the landmark that the mine is connected to |

#### TextPopup
| Method | Param | Return | Description |
| :--- | :--- | :--- | :--- |
| `DisplayTextAndTitle` | `string` title, `string` text | | Makes text popup with title and text visible. |
| `DisplayText` | `string` text | | Makes text popup with text visible. |

## Server API
`GET http://nvader.azurewebsites.net/goodies?lat={latitude}&lon={longitude}`
Returns a JSON object

    {"dist":252.96047122030745,
    "name":"Powelton Historic District",
    "description":"Powelton Village is a neighborhood of mostly Victorian, mostly twin homes in the West Philadelphia section of the United States city of Philadelphia, Pennsylvania. It is a national historic district which is part of University City. It extends north from Market Street to Spring Garden Street, east to 32nd Street, west to 40th and Spring Garden Streets, and to 44th and Market Streets.",
    "wikiArticle":"Powelton_Village,_Philadelphia",
    "buildDate":"1902"}
     

### Object Parameters
* `dist` - Distance to location (m)
* `name` - Name of landmark
* `description` - Short paragraph
* `wikiArticle` - Wikipedia article title
* `buildDate` - Time the landmark was originally built
