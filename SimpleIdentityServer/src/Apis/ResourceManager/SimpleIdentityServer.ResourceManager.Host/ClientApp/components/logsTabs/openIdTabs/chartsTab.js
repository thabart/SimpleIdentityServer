import React, { Component } from "react";
import { translate } from 'react-i18next';

class ChartsTab extends Component {
    constructor(props) {
        super(props);
    }

    render() {
        return (<div className="row">
        	<div className="col-md-6">
        		<div className="card">
			      <div className="card-header" data-target="#authorization" data-toggle="collapse">Authorization</div>
			      <div className="card-block collapse show" id="authorization"></div>
		      	</div>
        	</div>
        	<div className="col-md-6">
        		<div className="card">
			      <div className="card-header" data-target="#token" data-toggle="collapse">Token</div>
			      <div className="card-block collapse show" id="token">

			      </div>
		      	</div>
        	</div>
        	<div className="col-md-12">
        		<div className="card">
      				<div className="card-header" data-target="#errors" data-toggle="collapse">Errors</div>
      				<div className="card-block collapse show" id="errors">

      				</div>
        		</div>
        	</div>
        </div>);
    }
}

export default translate('common', { wait: process && !process.release })(ChartsTab);