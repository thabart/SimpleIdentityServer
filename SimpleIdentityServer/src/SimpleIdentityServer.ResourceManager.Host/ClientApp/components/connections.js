import React, { Component } from "react";
import { translate } from 'react-i18next';
import Modal from './modal';

class Connections extends Component {
    constructor(props) {
        super(props);
        this.myModal = null;
        this.toggleAddIdentityProviderModal = this.toggleAddIdentityProviderModal.bind(this);
        this.addIdentityProvider = this.addIdentityProvider.bind(this);
        this.selectIdentityProvider = this.selectIdentityProvider.bind(this);
        this.refreshConnections = this.refreshConnections.bind(this);
        this.state = {
            isAddIdentityProviderModalDisplayed: false,
            providerName: null,
            authProviders: []
        }
    }

    toggleAddIdentityProviderModal() {
        this.setState({
            isAddIdentityProviderModalDisplayed: !this.state.isAddIdentityProviderModalDisplayed
        });
    }

    addIdentityProvider() {

    }

    selectIdentityProvider(providerName) {
        this.setState({
            providerName: providerName
        });
    }

    refreshConnections() {
        this.setState({
            authProviders: [
                { name: 'facebook', is_enabled: true },
                { name: 'google', is_enabled: true },
                { name: 'eid', is_enabled: false }
            ]
        });
    }

    render() {
        var self = this;
        const { t } = this.props;
        var authProviders = [];
        if (this.state.authProviders) {
            this.state.authProviders.forEach(function (authProvider) {
                authProviders.push((<div className="col-md-3" key={authProvider.name}>
                    <div className="card">
                        <div className="card-block">
                            <h4>{authProvider.name}</h4>
                            {authProvider.is_enabled ? (<button className="btn btn-default">Disable</button>): (<button className="btn btn-default">Enable</button>)}                            
                        </div>
                    </div>
                </div>));
            });
        }

        return (<div>
            <h4>{t('connectionTitle')}</h4>
            <div className="margin-5">
                <button type="button" className="btn btn-default" onClick={this.toggleAddIdentityProviderModal}>{t('addIdentityProvider')}</button>
            </div>
            <div className="row">
                {authProviders}
            </div>
            {this.state.isAddIdentityProviderModalDisplayed && (
                <Modal title={t('addIdentityProviderModalTitle')} ref={c => this.myModal = c} handleHideModal={this.toggleAddIdentityProviderModal} onConfirmHandle={this.addIdentityProvider} confirmTxt={t('add')}>
                    <div>
                        <div className="form-group">
                            <label className="control-label">{t('name')}</label>
                            <input type="text" className="form-control" />
                        </div>
                        <div className="form-group">
                            <label className="control-label" dangerouslySetInnerHTML={{ __html: t('addCallbackPathLabel') }}></label>
                            <input type="text" className="form-control" />
                        </div>
                        <label>{t('chooseIdentityProviderLabel')}</label>
                        <div className="row identity-providers-selector">
                            <div className="col-md-3">
                                <div className={"card" + (this.state.providerName === "openid" ? " active" : "")} onClick={() => self.selectIdentityProvider('openid')}>
                                    <div className="card-block">
                                        <img src="/imgs/openid-icon-100x100.png" />
                                        <p dangerouslySetInnerHTML={{ __html: t('openIdIdProviderDescription')}}></p>
                                    </div>
                                </div>
                            </div>
                            <div className="col-md-3">
                                <div className={"card" + (this.state.providerName === "oauth2" ? " active" : "")} onClick={() => self.selectIdentityProvider('oauth2')}>
                                    <div className="card-block">
                                        <img src="/imgs/oauth.png" />
                                        <p dangerouslySetInnerHTML={{ __html: t('oauth2IdProviderDescription') }}></p>
                                    </div>
                                </div>
                            </div>
                            <div className="col-md-3">
                                <div className={"card" + (this.state.providerName === "adfs" ? " active" : "")} onClick={() => self.selectIdentityProvider('adfs')}>
                                    <div className="card-block">
                                        <img src="/imgs/adfs-azure.png" />
                                        <p dangerouslySetInnerHTML={{ __html: t('adfsIdProviderDescription') }}></p>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </Modal>
            )}
        </div>);
    }

    componentDidMount() {
        this.refreshConnections();
    }
}

export default translate('common', { wait: process && !process.release })(Connections);