import React, { Component } from "react";
import { translate } from 'react-i18next';
import { Chip, IconButton, Button, Paper, Select, MenuItem } from 'material-ui';
import Input, { InputLabel } from 'material-ui/Input';
import { FormControl, FormHelperText } from 'material-ui/Form';
import { withStyles } from 'material-ui/styles';
import Delete from '@material-ui/icons/Delete';

const styles = theme => ({
  margin: {
    margin: theme.spacing.unit
  }
});

class ChipsSelector extends Component {
	constructor(props) {
		super(props);
		this.handleProperty = this.handleProperty.bind(this);
		this.handleAddProperty = this.handleAddProperty.bind(this);
		this.handleRemoveProperty = this.handleRemoveProperty.bind(this);
        var property = '';
        if (props.input && props.input.type === 'select') {
            property = props.input.values[0].key;
        }

		this.state = {
			property: property,
            input: props.input ? props.input : {
                type: "text"
            },
			properties: props.properties ? props.properties :  []
		};
	}

	/**
	* Handle the property change.
	*/
    handleProperty(e)  {
        this.setState({
            [e.target.name]: e.target.value
        });
    }

    /**
    * Add a property.
    */
    handleAddProperty() {
        var self = this;
        var properties = self.state.properties;
        if (properties.indexOf(self.state.property) !== -1) {
            return;
        }

        properties.push(self.state.property);
        var property = '';
        if (self.state.input && self.state.input.type === 'select' && properties.length > 0) {
            property = properties[0];
        }

        self.setState({
            property: property,
            properties: properties
        });
    }

    /**
    * Delete a scope.
    */
    handleRemoveProperty(property) {
        const properties = this.state.properties;
        const propertyIndex = properties.indexOf(property);
        properties.splice(propertyIndex, 1);        
        this.setState({
            properties: properties
        });
    }

	render() {
		var self = this;
		const { t, classes } = self.props;
		var chips = [];
        if (self.state.properties) {
            self.state.properties.forEach(function(property) {
                chips.push((<Chip label={property} key={property} className={classes.margin} onDelete={() => self.handleRemoveProperty(property)} />));
            });
        }

        var formInput = (<FormControl className={classes.margin}>
                        <InputLabel htmlFor="property">{self.props.label}</InputLabel>
                        <Input id="name" value={self.state.property} name="property" onChange={self.handleProperty}  />
                    </FormControl>);
        if (self.state.input.type === "select") {
            var options = [];
            if (self.state.input.values) {
                self.state.input.values.forEach(function(value) {
                    options.push((<MenuItem value={value.key}>{t(value.label)}</MenuItem>))
                });
            }

            formInput = (<FormControl className={classes.margin}>
                <Select value={self.state.property} onChange={self.handleProperty} name="property">
                    {options}
                </Select>
            </FormControl>);
        }

		return (
			<div>
				<form onSubmit={(e) => { e.preventDefault(); self.handleAddProperty(); }}>
	            	{formInput}
	                <Button variant="raised" color="primary" onClick={this.handleAddProperty}>{t('add')}</Button>
	            </form>
	            <Paper>
	            	{chips}
	            </Paper>
	        </div>);
	}
}

export default translate('common', { wait: process && !process.release })(withStyles(styles)(ChipsSelector));