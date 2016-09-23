package improbable.natures
import improbable.corelib.natures.{BaseNature, NatureApplication, NatureDescription}
import improbable.corelibrary.transforms.TransformNature
import improbable.papi.entity.EntityPrefab
import improbable.papi.entity.behaviour.EntityBehaviourDescriptor
import improbable.player.{LocalPlayerCheck, Name}
import improbable.behaviours.{DelegateLocalPlayerCheckToClientBehaviour, DelegatePlayerControlsToOwnerBehaviour, PrintNameBehaviour}
import improbable.corelib.util.EntityOwner
import improbable.math.Vector3d
import improbable.papi.engine.EngineId
import improbable.player.controls.PlayerControls
import improbable.player.physical.Forces

object Player extends NatureDescription {

  override val dependencies = Set[NatureDescription](BaseNature, TransformNature)

  override def activeBehaviours: Set[EntityBehaviourDescriptor] = {
    Set(
      descriptorOf[PrintNameBehaviour],
      descriptorOf[DelegateLocalPlayerCheckToClientBehaviour],
      descriptorOf[DelegatePlayerControlsToOwnerBehaviour])
  }

  def apply(playerName: String, clientId: EngineId): NatureApplication = {
    application(
      states = Seq(
        Name(playerName),
        EntityOwner(Some(clientId)),
        LocalPlayerCheck(),
        Forces(forceMagnitude = 20.0f),
        PlayerControls(movementDirection = Vector3d.zero)
      ),
      natures = Seq(
        BaseNature(entityPrefab = EntityPrefab("Player"), isPhysical = true),
        TransformNature(globalPosition = Vector3d(0,0.5f,0))
      )
    )
  }
}