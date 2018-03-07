import React, { Component } from "react";
import { translate } from 'react-i18next';
import { withRouter } from 'react-router-dom';
import { JsonWebEncryptionTab, JsonWebSignatureTab } from './toolsTabs';

class Tools extends Component {
    constructor(props) {
        super(props);
        this.navigate = this.navigate.bind(this);
        this.state = {
            tabName: null
        };
    }

    navigate(e, tabName) {
        e.preventDefault();
        this.setState({
            tabName: tabName
        });
        this.props.history.push('/tools/' + tabName);
    }

    render() {
        var self = this;
        const { t } = self.props;
        return (<div>
            <ul className="nav nav-tabs">
                <li className="nav-item">
                    <a href="#" className="nav-link" onClick={(e) => self.navigate(e, "jws")}>{t('Jws')}</a>
                </li>
                <li className="nav-item">
                    <a href="#" className="nav-link" onClick={(e) => self.navigate(e, "jwe")}>{t('Jwe')}</a>
                </li>
            </ul>
            <div className="tab-content">
                {self.state.tabName === 'jws' && (<JsonWebSignatureTab />)}
                {self.state.tabName === 'jwe' && (<JsonWebEncryptionTab />)}
            </div>
        </div>);
    }

    componentDidMount() {
        var self = this;
        var action = self.props.match.params.action;
        if (!action || (action !== 'jwe' && action !== 'jws')) {
            action = 'jws';
        }

        self.setState({
            tabName: action
        });
    }
}

export default translate('common', { wait: process && !process.release })(withRouter(Tools));