public interface MeshOperation
{

    MeshOperations GetOperationType();

    void Execute(EditableMesh em);
}
