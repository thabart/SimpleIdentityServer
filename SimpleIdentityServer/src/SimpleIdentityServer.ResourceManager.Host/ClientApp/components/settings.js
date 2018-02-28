import React, { Component } from "react";
import { translate } from 'react-i18next';
import { TokenTab, TwilioTab, EmailTab } from './settingsTabs';
import { withRouter } from 'react-router-dom';

class Settings extends Component {
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
        this.props.history.push('/settings/' + tabName);
    }

    render() {
        var self = this;
        const { t } = self.props;
        return (<div>
            <ul className="nav nav-tabs">
                <li className="nav-item">
                    <a href="#" className="nav-link" onClick={(e) => self.navigate(e, "tokens")}>{t('tokens')}</a>
                </li>
                <li className="nav-item">
                    <a href="#" className="nav-link" onClick={(e) => self.navigate(e, "email")}>{t('email')}</a>
                </li>
                <li className="nav-item">
                    <a href="#" className="nav-link" onClick={(e) => self.navigate(e, "twilio")}>{t('Twilio')}</a>
                </li>
            </ul>
            <div className="tab-content">
                {self.state.tabName === 'tokens' && (<TokenTab />)}
                {self.state.tabName === 'email' && (<EmailTab />)}
                {self.state.tabName === 'twilio' && (<TwilioTab />)}
            </div>
        </div>);
    }

    componentDidMount() {
        var self = this;
        var action = self.props.match.params.action;
        if (!action || (action !== 'tokens' && action !== 'email' && action !== 'twilio')) {
            action = 'tokens';
        }

        self.setState({
            tabName: action
        });
    }
}

export default translate('common', { wait: process && !process.release })(withRouter(Settings));