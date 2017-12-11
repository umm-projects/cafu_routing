﻿using CAFU.Core.Domain;
using CAFU.Routing.Domain.Model;
using CAFU.Routing.Domain.Repository;
using CAFU.Routing.Domain.Translator;
using UniRx;
using UnityEngine.SceneManagement;

namespace CAFU.Routing.Domain.UseCase {

    public class RoutingUseCase : IUseCaseAsSingleton, IUseCaseBuilder {

        public void Build() {
            this.LoadSceneSubject = new Subject<SceneModel>();
            this.UnloadSceneSubject = new Subject<SceneModel>();
            this.RoutingRepository = new RoutingRepository();
            this.RoutingTranslator = new RoutingTranslator();
        }

        private RoutingRepository RoutingRepository { get; set; }

        private RoutingTranslator RoutingTranslator { get; set; }

        private Subject<SceneModel> LoadSceneSubject { get; set; }

        private Subject<SceneModel> UnloadSceneSubject { get; set; }

        public void LoadScene(string sceneName, LoadSceneMode loadSceneMode) {
            this.LoadSceneAsObservable(sceneName, loadSceneMode).Subscribe();
        }

        public void UnloadScene(string sceneName) {
            this.UnloadSceneAsObservable(sceneName).Subscribe();
        }

        public IObservable<SceneModel> LoadSceneAsObservable(string sceneName, LoadSceneMode loadSceneMode) {
            IObservable<SceneModel> stream = this.RoutingRepository
                .LoadSceneAsObservable(sceneName, loadSceneMode)
                .SelectMany(x => this.RoutingTranslator.TranslateAsync(x))
                .Share();
            // OnComplete を流してしまうと、Subject が閉じてしまうので OnNext, OnError のみを流す
            stream
                .Subscribe(
                    this.LoadSceneSubject.OnNext,
                    this.LoadSceneSubject.OnError
                );
            return stream;
        }

        public IObservable<SceneModel> UnloadSceneAsObservable(string sceneName) {
            IObservable<SceneModel> stream = this.RoutingRepository
                .UnloadSceneAsObservable(sceneName)
                .SelectMany(x => this.RoutingTranslator.TranslateAsync(x))
                .Share();
            // OnComplete を流してしまうと、Subject が閉じてしまうので OnNext, OnError のみを流す
            stream
                .Subscribe(
                    this.UnloadSceneSubject.OnNext,
                    this.UnloadSceneSubject.OnError
                );
            return stream;
        }

        public IObservable<SceneModel> OnLoadSceneAsObservable() {
            return this.LoadSceneSubject.AsObservable();
        }

        public IObservable<SceneModel> OnLoadSceneAsObservable(string sceneName) {
            return this.OnLoadSceneAsObservable().Where(x => x.Name == sceneName).AsObservable();
        }

        public IObservable<SceneModel> OnUnloadSceneAsObservable() {
            return this.UnloadSceneSubject.AsObservable();
        }

        public IObservable<SceneModel> OnUnloadSceneAsObservable(string sceneName) {
            return this.OnUnloadSceneAsObservable().Where(x => x.Name == sceneName).AsObservable();
        }

    }

}
