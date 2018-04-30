import React, { Component } from "react";
import { translate } from 'react-i18next';
import { ScopeService } from '../services';
import { CircularProgress, IconButton } from 'material-ui';
import { DisplayScope } from './common';
import Save from '@material-ui/icons/Save';

class AddScope extends Component {
    constructor(props) {
        super(props);
        this.saveScope = this.saveScope.bind(this);
        this.state = {
            type: '',
        	scope: {
                type: 0                
            },
        	isLoading: true
        };
    }

    /**
    * Save the scope.
    */
    saveScope() {
        var self = this;
        self.setState({
            isLoading: true
        });
        ScopeService.add(self.state.scope, self.state.type).then(function() {
            self.setState({
                isLoading: false
            });
        }).catch(function(e) {
            self.setState({
                isLoading: false
            });
        });
    }

    /**
    * Display the view.
    */
    render() {
        var self = this;
        const { t, classes } = self.props;
        return (<div className="block">
            <div className="block-header">
                <h4>{t('scopeTitle')}</h4>
                <i>{t('scopeShortDescription')}</i>
            </div>
            <div className="card">
                <div className="header">
                    <h4 style={{display: "inline-block"}}>{t('addScope')}</h4>                    
                    <div style={{float: "right"}}>                        
                        <IconButton onClick={this.saveScope}>
                            <Save />
                        </IconButton>
                    </div>
                </div>
                <div className="body">
                    { self.state.isLoading ? (<CircularProgress />) : (
                        <DisplayScope type={self.state.type} scope={self.state.scope} />
                    ) }
                </div>
            </div>
        </div>);
    }

    componentDidMount() {
        var self = this;
        self.setState({
            type: self.props.match.params.type
        }, () => {
            self.setState({
                isLoading: false
            });
        });
    }
}

export default translate('common', { wait: process && !process.release })(AddScope);