using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class Instanced : MonoBehaviour
{
	[Serializable]
	public class TriggerEvent : UnityEvent<float>
	{
	}

	[FormerlySerializedAs("onUpdate")]
	[SerializeField]
	protected TriggerEvent m_onUpdate = new TriggerEvent();

	public TriggerEvent onUpdate
	{
		get
		{
			return m_onUpdate;
		}
		set
		{
			m_onUpdate = value;
		}
	}
}
