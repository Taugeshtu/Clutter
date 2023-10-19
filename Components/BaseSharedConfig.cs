using UnityEngine;

[System.Serializable]
public abstract class BaseConfig {}

public abstract class BaseSharedConfig : ScriptableObject {
	public BaseConfig Config;
}

public abstract class BaseSharedConfig<T> : BaseSharedConfig where T: BaseConfig {
	public new T Config;
}
