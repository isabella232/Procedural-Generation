package improbable.behaviours

import improbable.corelib.util.EntityOwnerDelegation.entityOwnerDelegation
import improbable.papi.entity.{Entity, EntityBehaviour}
import improbable.player.controls.PlayerControls

class DelegatePlayerControlsToOwnerBehaviour(entity: Entity) extends EntityBehaviour {

  override def onReady(): Unit = {
    entity.delegateStateToOwner[PlayerControls]
  }
}
