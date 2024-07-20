mergeInto(LibraryManager.library, {
    Awake: function() {
        try {
            window.dispatchReactUnityEvent("Awake");
        } catch (e) {
            console.warn("Failed to dispatch event.");
        }
    }
})