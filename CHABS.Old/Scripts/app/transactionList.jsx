var TransactionLineItem = React.createClass({
	render: function() {
		return (
			<tr>
				<td>{this.props.Transaction.Date}</td>
				<td>{this.props.Transaction.Amount}</td>
				<td>{this.props.Transaction.Description}</td>
			</tr>
		);
	}
});

var TransactionList = React.createClass({
	getInitialState: function() {
		return {data: []};
	},
	componentWillMount: function() {
		$.ajax({
			url: '',
			success: function(data) {

			}
		});
		this.setState({ data: [{Date: 'test', Amount: 'test', Description: 'test'}]});
	},
	render: function () {
		var items = this.state.data.map(function(transaction) {
			return (
				<TransactionLineItem Transaction={transaction} />
			)
		});
		return (
			<table className="table table-striped">
				<tbody>
				<tr>
					<th>Date</th>
					<th>Amount</th>
					<th>Description</th>
					<th>Source</th>
					<th>Category</th>
				</tr>
				{items}
				</tbody>
			</table>
		);
	}
});