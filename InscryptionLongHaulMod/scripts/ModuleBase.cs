using System;
using System.Collections.Generic;
using System.Text;

namespace LongHaulMod {
	public class ModuleBase {

		public MainPlugin plugin;

		public virtual void Awake() { }

		public virtual void Start() { }

		public ModuleBase(MainPlugin plugin) {
			this.plugin = plugin;
			MainPlugin.logger.LogInfo($"{this.GetType().Name} has been initialized");
		}

	}
}
