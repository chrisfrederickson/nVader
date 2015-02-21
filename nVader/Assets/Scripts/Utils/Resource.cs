public class Resource {
	private string Name;
	private int Value;

	public Resource(string n, int v) {
		Name = n;
		Value = v;
	}
	public string GetResourceType() {
		return Name;
	}
	public int GetResourceValue() {
		return Value;
	}
	public void AddResourcesOfThisType(int count) {
		Value = Value + count;
	}
}
